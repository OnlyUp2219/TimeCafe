namespace UserProfile.TimeCafe.Infrastructure.Services;

public class UserRegisteredConsumer(IUserRepositories userService, ILogger<UserRegisteredConsumer> logger) : IConsumer<UserRegisteredEvent>
{
    private readonly IUserRepositories _userService = userService;
    private readonly ILogger<UserRegisteredConsumer> _logger = logger;

    //public async Task Consume(ConsumeContext<UserRegistered> context)
    //{
    //    try
    //    {
    //        await _userService.CreateEmptyAsync(context.Message.UserId, context.CancellationToken);
    //        _logger.LogInformation("Профиль для UserId {UserId} успешно создан", context.Message.UserId);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Ошибка при обработке UserRegistered для UserId {UserId}", context.Message.UserId);
    //    }
    //}

    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation(
            "Получено событие регистрации пользователя! UserId: {UserId}, Email: {Email}",
            evt.UserId,
            evt.Email);

        return Task.CompletedTask;
    }
}
