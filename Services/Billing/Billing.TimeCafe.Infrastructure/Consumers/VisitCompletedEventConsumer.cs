namespace Billing.TimeCafe.Infrastructure.Consumers;

public class VisitCompletedEventConsumer(
    IBalanceRepository balanceRepository,
    ITransactionRepository transactionRepository,
    ILogger<VisitCompletedEventConsumer> logger) : IConsumer<VisitCompletedEvent>
{
    private readonly IBalanceRepository _balanceRepository = balanceRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ILogger _logger = logger;

    public async Task Consume(ConsumeContext<VisitCompletedEvent> context)
    {
        var evt = context.Message;

        try
        {
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
                _logger.LogWarning(
                    "Транзакция для визита {VisitId} уже обработана",
                    evt.VisitId);
                return;
            }

            if (balance.CurrentBalance < evt.Amount)
            {
                balance.Debt += (evt.Amount - balance.CurrentBalance);
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

            _logger.LogInformation(
                "Визит {VisitId} пользователя {UserId} успешно обработан. Списано: {Amount}₽, Баланс: {Balance}₽",
                evt.VisitId, evt.UserId, evt.Amount, balance.CurrentBalance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Ошибка при обработке завершённого визита {VisitId} пользователя {UserId}",
                evt.VisitId, evt.UserId);
            throw;
        }
    }
}
