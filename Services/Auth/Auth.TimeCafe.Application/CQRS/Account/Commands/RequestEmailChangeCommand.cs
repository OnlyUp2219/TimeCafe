namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record RequestEmailChangeCommand(string UserId, string NewEmail, bool SendEmail = true) : IRequest<RequestEmailChangeResult>;

public record RequestEmailChangeResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    string? CallbackUrl = null) : ICqrsResultV2
{
    public static RequestEmailChangeResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 401);

    public static RequestEmailChangeResult EmailUnchanged() =>
        new(false, Code: "EmailUnchanged", Message: "Email уже установлен", StatusCode: 400);

    public static RequestEmailChangeResult EmailAlreadyInUse() =>
        new(false, Code: "EmailAlreadyInUse", Message: "Email уже используется", StatusCode: 400);

    public static RequestEmailChangeResult Sent() =>
        new(true, Message: "Ссылка для подтверждения отправлена");

    public static RequestEmailChangeResult MockCallback(string callbackUrl) =>
        new(true, Message: "CallbackUrl сгенерирован", CallbackUrl: callbackUrl);

    public static RequestEmailChangeResult SendFailed() =>
        new(false, Code: "SendFailed", Message: "Ошибка при отправке письма", StatusCode: 500);
}

public class RequestEmailChangeCommandValidator : AbstractValidator<RequestEmailChangeCommand>
{
    public RequestEmailChangeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
            .NotNull().WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");

        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("Email не может быть пустым")
            .EmailAddress().WithMessage("Неверный формат Email");
    }
}

public class RequestEmailChangeCommandHandler(
    UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender,
    IOptions<PostmarkOptions> postmarkOptions,
    IHostEnvironment environment,
    ILogger<RequestEmailChangeCommandHandler> logger) : IRequestHandler<RequestEmailChangeCommand, RequestEmailChangeResult>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailSender<ApplicationUser> _emailSender = emailSender;
    private readonly PostmarkOptions _postmarkOptions = postmarkOptions.Value;
    private readonly IHostEnvironment _environment = environment;
    private readonly ILogger<RequestEmailChangeCommandHandler> _logger = logger;

    public async Task<RequestEmailChangeResult> Handle(RequestEmailChangeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return RequestEmailChangeResult.UserNotFound();

        var currentEmail = user.Email ?? string.Empty;
        if (string.Equals(currentEmail, request.NewEmail, StringComparison.OrdinalIgnoreCase))
            return _environment.IsDevelopment()
                ? RequestEmailChangeResult.EmailUnchanged()
                : RequestEmailChangeResult.Sent();

        var existing = await _userManager.FindByEmailAsync(request.NewEmail);
        if (existing != null && existing.Id != user.Id)
            return _environment.IsDevelopment()
                ? RequestEmailChangeResult.EmailAlreadyInUse()
                : RequestEmailChangeResult.Sent();

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var encodedEmail = Uri.EscapeDataString(request.NewEmail);
        var callbackUrl = $"{_postmarkOptions.FrontendBaseUrl}/confirm-email?userId={user.Id}&token={encodedToken}&newEmail={encodedEmail}";

        if (request.SendEmail)
        {
            try
            {
                await _emailSender.SendConfirmationLinkAsync(user, request.NewEmail, callbackUrl);
                return RequestEmailChangeResult.Sent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке письма на {Email}", request.NewEmail);
                return RequestEmailChangeResult.SendFailed();
            }
        }

        return RequestEmailChangeResult.MockCallback(callbackUrl);
    }
}
