namespace UserProfile.TimeCafe.Infrastructure.Services;

public class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly IUserRepositories _userService;

    public UserRegisteredConsumer(IUserRepositories userService)
    {
        _userService = userService;
    }

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        
        var message = context.Message;


        //await _profileService.CreateEmptyAsync(context.Message.UserId);

    }
}
