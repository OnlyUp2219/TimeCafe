namespace UserProfile.TimeCafe.Infrastructure.Services;

public class UserRegisteredConsumer(IUserRepositories userService, Logger<UserRegisteredConsumer> logger) : IConsumer<UserRegistered>
{
    private readonly IUserRepositories _userService = userService;
    private readonly Logger<UserRegisteredConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        try
        {
            await _userService.CreateEmptyAsync(context.Message.UserId, context.CancellationToken);
            _logger.LogInformation("Профиль для UserId {UserId} успешно создан", context.Message.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке UserRegistered для UserId {UserId}", context.Message.UserId);
        }
    }
}
