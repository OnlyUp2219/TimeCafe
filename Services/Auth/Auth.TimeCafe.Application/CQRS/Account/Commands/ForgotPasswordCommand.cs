namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record ForgotPasswordCommand(string Email, bool SendEmail = true) : IRequest<ForgotPasswordResult>;

public record ForgotPasswordResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    string? CallbackUrl = null) : ICqrsResultV2
{
    public static ForgotPasswordResult SuccessSilent() =>
       new(true, Message: "Если пользователь существует, письмо отправлено");

    public static ForgotPasswordResult Sent() =>
        new(true, Message: "Ссылка для сброса пароля отправлена");

    public static ForgotPasswordResult SendFailed() =>
        new(false, Code: "EmailSendFailed", Message: "Ошибка при отправке письма", StatusCode: 500);

    public static ForgotPasswordResult MockCallback(string callbackUrl) =>
        new(true, Message: "CallbackUrl сгенерирован", CallbackUrl: callbackUrl);
}

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email");
    }
}

public class ForgotPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender,
    IOptions<PostmarkOptions> postmarkOptions,
    ILogger<ForgotPasswordCommandHandler> logger) : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailSender<ApplicationUser> _emailSender = emailSender;
    private readonly PostmarkOptions _postmarkOptions = postmarkOptions.Value;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger = logger;

    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return ForgotPasswordResult.SuccessSilent();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = $"{_postmarkOptions.FrontendBaseUrl}/confirm-reset?email={request.Email}&code={encodedToken}";

        if (request.SendEmail)
        {
            try
            {
                await _emailSender.SendPasswordResetLinkAsync(user, request.Email, callbackUrl);
                return ForgotPasswordResult.Sent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке письма на {Email}", request.Email);
                return ForgotPasswordResult.SendFailed();
            }
        }

        return ForgotPasswordResult.MockCallback(callbackUrl);
    }
}
