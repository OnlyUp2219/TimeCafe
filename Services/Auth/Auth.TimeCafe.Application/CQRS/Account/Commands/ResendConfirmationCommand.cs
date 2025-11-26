namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record ResendConfirmationCommand(string Email, bool SendEmail = true) : IRequest<ResendConfirmationResult>;

public record ResendConfirmationResult(bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    string? CallbackUrl = null) : ICqrsResultV2
{
    public static ResendConfirmationResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 401);

    public static ResendConfirmationResult EmailAlreadyConfirmed() =>
        new(false, Code: "EmailAlreadyConfirmed", Message: "Email уже подтвержден", StatusCode: 400);

    public static ResendConfirmationResult Sent() =>
    new(true, Message: "Ссылка для сброса пароля отправлена");

    public static ResendConfirmationResult MockCallback(string callbackUrl) =>
    new(true, Message: "CallbackUrl сгенерирован", CallbackUrl: callbackUrl);

    public static ResendConfirmationResult SendFailed() =>
        new(false, Code: "SendFailed", Message: "Ошибка при отправке письма", StatusCode: 500);
}

public class ResendConfirmationCommandValidator : AbstractValidator<ResendConfirmationCommand>
{
    public ResendConfirmationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email не может быть пустым")
            .EmailAddress().WithMessage("Неверный формат Email");
    }
}


public class ResendConfirmationCommandHandler(
    UserManager<IdentityUser> userManager,
    IEmailSender<IdentityUser> emailSender,
    IOptions<PostmarkOptions> postmarkOptions,
    ILogger<ResendConfirmationCommandHandler> logger) : IRequestHandler<ResendConfirmationCommand, ResendConfirmationResult>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IEmailSender<IdentityUser> _emailSender = emailSender;
    private readonly PostmarkOptions _postmarkOptions = postmarkOptions.Value;
    private readonly ILogger<ResendConfirmationCommandHandler> _logger = logger;


    public async Task<ResendConfirmationResult> Handle(ResendConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return ResendConfirmationResult.UserNotFound();
        if (user.EmailConfirmed)
            return ResendConfirmationResult.EmailAlreadyConfirmed();

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = $"{_postmarkOptions.FrontendBaseUrl}/confirm-email?userId={user.Id}&token={encodedToken}";

        if (request.SendEmail)
        {
            try
            {
                await _emailSender.SendConfirmationLinkAsync(user, request.Email, callbackUrl);
                return ResendConfirmationResult.Sent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке письма на {Email}", request.Email);
                return ResendConfirmationResult.SendFailed();
            }
        }

        return ResendConfirmationResult.MockCallback(callbackUrl);
    }
}
