namespace Billing.TimeCafe.Infrastructure.Consumers;

using Npgsql;

public class VisitCompletedEventConsumer(
    ApplicationDbContext db,
    IBalanceRepository balanceRepository,
    ITransactionRepository transactionRepository,
    ILogger<VisitCompletedEventConsumer> logger) : IConsumer<VisitCompletedEvent>
{
    private readonly ApplicationDbContext _db = db;
    private readonly IBalanceRepository _balanceRepository = balanceRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ILogger _logger = logger;

    public async Task Consume(ConsumeContext<VisitCompletedEvent> context)
    {
        var evt = context.Message;

        await using var dbTransaction = await _db.Database.BeginTransactionAsync(context.CancellationToken);
        try
        {
            var saga = await _db.VisitBillingSagas
                .FirstOrDefaultAsync(x => x.VisitId == evt.VisitId, context.CancellationToken);

            if (saga is not null && (saga.Status == VisitBillingSagaStatus.Completed || saga.Status == VisitBillingSagaStatus.Compensated))
            {
                _logger.LogInformation("Сага для визита {VisitId} уже завершена со статусом {Status}", evt.VisitId, saga.Status);
                return;
            }

            if (saga is null)
            {
                saga = new VisitBillingSagaState
                {
                    VisitId = evt.VisitId,
                    UserId = evt.UserId,
                    Amount = evt.Amount,
                    Status = VisitBillingSagaStatus.Pending,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                _db.VisitBillingSagas.Add(saga);
                await _db.SaveChangesAsync(context.CancellationToken);
            }
            else
            {
                saga.Status = VisitBillingSagaStatus.Pending;
                saga.FailureReason = null;
                saga.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(context.CancellationToken);
            }

            var balance = await _balanceRepository.GetByUserIdAsync(evt.UserId, context.CancellationToken);

            if (balance == null)
            {
                balance = new Balance(evt.UserId);
                balance = await _balanceRepository.CreateAsync(balance, context.CancellationToken);
            }

            var duplicate = await _transactionRepository.ExistsBySourceAsync(
                TransactionSource.Visit,
                evt.VisitId,
                context.CancellationToken);

            if (duplicate)
            {
                saga.Status = VisitBillingSagaStatus.Completed;
                saga.CompletedAt = DateTimeOffset.UtcNow;
                saga.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(context.CancellationToken);
                await dbTransaction.CommitAsync(context.CancellationToken);

                _logger.LogWarning(
                    "Транзакция для визита {VisitId} уже обработана",
                    evt.VisitId);
                return;
            }

            if (balance.CurrentBalance < evt.Amount)
            {
                balance.Debt += evt.Amount - balance.CurrentBalance;
                balance.CurrentBalance = 0;
            }
            else
            {
                balance.CurrentBalance -= evt.Amount;
            }

            balance.TotalSpent += evt.Amount;
            balance.LastUpdated = DateTimeOffset.UtcNow;

            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                UserId = evt.UserId,
                Amount = evt.Amount,
                Type = TransactionType.Withdrawal,
                Source = TransactionSource.Visit,
                SourceId = evt.VisitId,
                Status = TransactionStatus.Completed,
                Comment = $"Оплата визита #{evt.VisitId}",
                CreatedAt = evt.CompletedAt,
                BalanceAfter = balance.CurrentBalance
            };

            await _balanceRepository.UpdateAsync(balance, context.CancellationToken);
            await _transactionRepository.CreateAsync(transaction, context.CancellationToken);

            saga.Status = VisitBillingSagaStatus.Completed;
            saga.TransactionId = transaction.TransactionId;
            saga.CompletedAt = DateTimeOffset.UtcNow;
            saga.UpdatedAt = DateTimeOffset.UtcNow;
            saga.FailureReason = null;
            await _db.SaveChangesAsync(context.CancellationToken);

            await dbTransaction.CommitAsync(context.CancellationToken);

            _logger.LogInformation(
                "Визит {VisitId} пользователя {UserId} успешно обработан. Списано: {Amount}₽, Баланс: {Balance}₽",
                evt.VisitId, evt.UserId, evt.Amount, balance.CurrentBalance);
        }
        catch (DbUpdateException ex) when (IsDuplicateSourceKeyViolation(ex))
        {
            await dbTransaction.RollbackAsync(context.CancellationToken);
            _db.ChangeTracker.Clear();
            await MarkSagaCompensatedAsync(evt, "duplicate source transaction", context.CancellationToken);

            _logger.LogWarning(
                "Дубликат транзакции для визита {VisitId}: компенсация применена",
                evt.VisitId);
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync(context.CancellationToken);
            _db.ChangeTracker.Clear();
            await MarkSagaFailedAsync(evt, ex.Message, context.CancellationToken);

            _logger.LogError(ex,
                "Ошибка при обработке завершённого визита {VisitId} пользователя {UserId}",
                evt.VisitId, evt.UserId);
            throw;
        }
    }

    private static bool IsDuplicateSourceKeyViolation(DbUpdateException ex)
    {
        if (ex.InnerException is not PostgresException postgres)
            return false;

        return postgres.SqlState == PostgresErrorCodes.UniqueViolation
            && string.Equals(postgres.ConstraintName, "UX_Transactions_Source_SourceId_NotNull", StringComparison.Ordinal);
    }

    private async Task MarkSagaCompensatedAsync(VisitCompletedEvent evt, string reason, CancellationToken ct)
    {
        var saga = await _db.VisitBillingSagas.FirstOrDefaultAsync(x => x.VisitId == evt.VisitId, ct);

        if (saga is null)
        {
            saga = new VisitBillingSagaState
            {
                VisitId = evt.VisitId,
                UserId = evt.UserId,
                Amount = evt.Amount,
                Status = VisitBillingSagaStatus.Compensated,
                FailureReason = reason,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CompensatedAt = DateTimeOffset.UtcNow
            };

            _db.VisitBillingSagas.Add(saga);
        }
        else
        {
            saga.Status = VisitBillingSagaStatus.Compensated;
            saga.FailureReason = reason;
            saga.CompensatedAt = DateTimeOffset.UtcNow;
            saga.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task MarkSagaFailedAsync(VisitCompletedEvent evt, string reason, CancellationToken ct)
    {
        var saga = await _db.VisitBillingSagas.FirstOrDefaultAsync(x => x.VisitId == evt.VisitId, ct);

        if (saga is null)
        {
            saga = new VisitBillingSagaState
            {
                VisitId = evt.VisitId,
                UserId = evt.UserId,
                Amount = evt.Amount,
                Status = VisitBillingSagaStatus.Failed,
                FailureReason = reason,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _db.VisitBillingSagas.Add(saga);
        }
        else
        {
            saga.Status = VisitBillingSagaStatus.Failed;
            saga.FailureReason = reason;
            saga.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }
}
