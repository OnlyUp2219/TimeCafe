namespace Billing.TimeCafe.Infrastructure.Consumers;

public sealed class VisitCompletedEventConsumer(
    IUnitOfWork uow,
    ApplicationDbContext db,
    ILogger<VisitCompletedEventConsumer> logger) : IConsumer<VisitCompletedEvent>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ApplicationDbContext _db = db;
    private readonly ILogger _logger = logger;

    public async Task Consume(ConsumeContext<VisitCompletedEvent> context)
    {
        var evt = context.Message;
        var cancellationToken = context.CancellationToken;

        await using var dbTransaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var saga = await _db.VisitBillingSagas
                .FirstOrDefaultAsync(x => x.VisitId == evt.VisitId, cancellationToken);

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
                await _db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                saga.Status = VisitBillingSagaStatus.Pending;
                saga.FailureReason = null;
                saga.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
            }

            var balance = await _uow.Balances.GetByIdAsync(evt.UserId, cancellationToken);

            if (balance == null)
            {
                balance = Balance.Create(evt.UserId);
                await _uow.Balances.CreateAsync(balance, cancellationToken);
            }

            var duplicate = await _uow.Transactions.ExistsBySourceAsync(
                TransactionSource.Visit,
                evt.VisitId,
                cancellationToken);

            if (duplicate)
            {
                saga.Status = VisitBillingSagaStatus.Completed;
                saga.CompletedAt = DateTimeOffset.UtcNow;
                saga.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
                await dbTransaction.CommitAsync(cancellationToken);

                _logger.LogWarning("Транзакция для визита {VisitId} уже обработана", evt.VisitId);
                return;
            }

            var withdrawResult = balance.Withdraw(evt.Amount);
            if (withdrawResult.IsFailed)
            {
                throw new InvalidOperationException($"Ошибка списания: {withdrawResult.Errors[0].Message}");
            }

            var transaction = Transaction.CreateWithdrawal(evt.UserId, evt.Amount, TransactionSource.Visit, evt.VisitId, $"Оплата визита #{evt.VisitId}");
            transaction.CreatedAt = evt.CompletedAt;
            transaction.BalanceAfter = balance.CurrentBalance;

            await _uow.Balances.UpdateAsync(balance, cancellationToken);
            await _uow.Transactions.CreateAsync(transaction, cancellationToken);

            saga.Status = VisitBillingSagaStatus.Completed;
            saga.TransactionId = transaction.TransactionId;
            saga.CompletedAt = DateTimeOffset.UtcNow;
            saga.UpdatedAt = DateTimeOffset.UtcNow;
            saga.FailureReason = null;
            await _db.SaveChangesAsync(cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            Billing.TimeCafe.Application.Metrics.BillingMetrics.Revenue.Inc((double)evt.Amount);

            _logger.LogInformation(
                "Визит {VisitId} пользователя {UserId} успешно обработан. Списано: {Amount}₽, Баланс: {Balance}₽",
                evt.VisitId, evt.UserId, evt.Amount, balance.CurrentBalance);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation("UX_Transactions_Source_SourceId_NotNull"))
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            _db.ChangeTracker.Clear();
            await MarkSagaCompensatedAsync(evt, "duplicate source transaction", cancellationToken);

            _logger.LogWarning("Дубликат транзакции для визита {VisitId}: компенсация применена", evt.VisitId);
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            _db.ChangeTracker.Clear();
            await MarkSagaFailedAsync(evt, ex.Message, cancellationToken);

            _logger.LogError(ex, "Ошибка при обработке завершённого визита {VisitId} пользователя {UserId}", evt.VisitId, evt.UserId);
            throw;
        }
    }

    private async Task MarkSagaCompensatedAsync(VisitCompletedEvent evt, string reason, CancellationToken cancellationToken)
    {
        var saga = await _db.VisitBillingSagas.FirstOrDefaultAsync(x => x.VisitId == evt.VisitId, cancellationToken);

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

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task MarkSagaFailedAsync(VisitCompletedEvent evt, string reason, CancellationToken cancellationToken)
    {
        var saga = await _db.VisitBillingSagas.FirstOrDefaultAsync(x => x.VisitId == evt.VisitId, cancellationToken);

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

        await _db.SaveChangesAsync(cancellationToken);
    }
}
