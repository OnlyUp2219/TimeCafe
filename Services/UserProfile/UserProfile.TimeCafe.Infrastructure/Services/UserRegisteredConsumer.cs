namespace UserProfile.TimeCafe.Infrastructure.Services;

public class UserRegisteredConsumer(IUserRepositories userService, ILogger<UserRegisteredConsumer> logger) : IConsumer<UserRegisteredEvent>
{
    private readonly IUserRepositories _userService = userService;
    private readonly ILogger<UserRegisteredConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var evt = context.Message;

        try
        {
            await _userService.CreateEmptyAsync(evt.UserId, context.CancellationToken);
            _logger.LogInformation(
                "Профиль создан/проверен для UserId {UserId} после регистрации (Email: {Email})",
                evt.UserId,
                evt.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании профиля для UserId {UserId} после регистрации", evt.UserId);
            throw;
        }
    }
}
