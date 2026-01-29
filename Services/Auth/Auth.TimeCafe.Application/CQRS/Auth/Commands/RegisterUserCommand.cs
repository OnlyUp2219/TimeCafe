
namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record RegisterUserCommand(string Username, string Email, string Password, bool SendEmail = true) : IRequest<RegisterUserResult>;

public record RegisterUserResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    string? CallbackUrl = null) : ICqrsResultV2
{
    public static RegisterUserResult Error(List<ErrorItem>? errorItems) =>
        new(false, Code: "RegistrationError", Message: "Ошибка при регистрации", StatusCode: 400,
            Errors: errorItems);
    public static RegisterUserResult SuccessResult(string callbackUrl) =>
        new(true, Message: "Пользователь создан и письмо отправлено", CallbackUrl: callbackUrl);
}

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Логин обязателен");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен");
    }
}

public class RegisterUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender,
    IOptions<PostmarkOptions> postmarkOptions,
    IPublishEndpoint publishEndpoint) : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailSender<ApplicationUser> _emailSender = emailSender;
    private readonly PostmarkOptions _postmarkOptions = postmarkOptions.Value;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            List<ErrorItem> errs = [.. createResult.Errors.Select(e => new ErrorItem(e.Code, e.Description))];
            return RegisterUserResult.Error(errs);
        }

        await _publishEndpoint.Publish(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty
        }, cancellationToken);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = $"{_postmarkOptions.FrontendBaseUrl}/confirm-email?userId={user.Id}&token={encodedToken}";

        if (request.SendEmail)
        {
            await _emailSender.SendConfirmationLinkAsync(user, request.Email, callbackUrl);
        }

        return RegisterUserResult.SuccessResult(callbackUrl);
    }
}
