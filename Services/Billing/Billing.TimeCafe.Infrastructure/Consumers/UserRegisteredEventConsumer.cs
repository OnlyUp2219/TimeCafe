namespace Billing.TimeCafe.Infrastructure.Consumers;

public class UserRegisteredEventConsumer(IBalanceRepository balanceRepository, ILogger<UserRegisteredEventConsumer> logger)
    : IConsumer<UserRegisteredEvent>
{
    private readonly IBalanceRepository _balanceRepository = balanceRepository;
    private readonly ILogger _logger = logger;

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var evt = context.Message;

        try
        {
            var exists = await _balanceRepository.ExistsAsync(evt.UserId, context.CancellationToken);
            if (exists)
            {
                _logger.LogInformation("Баланс для пользователя {UserId} уже существует, пропускаем", evt.UserId);
                return;
            }

            var balance = new Balance
            {
                UserId = evt.UserId,
                CurrentBalance = 0,
                TotalDeposited = 0,
                TotalSpent = 0,
                Debt = 0,
                LastUpdated = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _balanceRepository.CreateAsync(balance, context.CancellationToken);

            _logger.LogInformation("Баланс создан для пользователя {UserId} после регистрации", evt.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании баланса для пользователя {UserId} после регистрации", evt.UserId);
            throw;
        }
    }
}
