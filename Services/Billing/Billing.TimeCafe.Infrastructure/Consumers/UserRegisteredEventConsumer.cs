namespace Billing.TimeCafe.Infrastructure.Consumers;

public sealed class UserRegisteredEventConsumer(IUnitOfWork uow, ILogger<UserRegisteredEventConsumer> logger)
    : IConsumer<UserRegisteredEvent>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ILogger _logger = logger;

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var evt = context.Message;

        var exists = await _uow.Balances.ExistsAsync(evt.UserId, context.CancellationToken);
        if (exists)
        {
            _logger.LogInformation("Баланс для пользователя {UserId} уже существует, пропускаем", evt.UserId);
            return;
        }

        var balance = Balance.Create(evt.UserId);
        await _uow.Balances.CreateAsync(balance, context.CancellationToken);
        await _uow.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Баланс создан для пользователя {UserId} после регистрации", evt.UserId);
    }
}
