namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record ForgotPasswordCommand(string Email, bool SendEmail = true) : IRequest<ForgotPasswordResult>;

public record ForgotPasswordResult(bool Success, string? CallbackUrl = null, string? Message = null, string? Error = null);

public class ForgotPasswordCommandHandler(
    UserManager<IdentityUser> userManager,
    IEmailSender<IdentityUser> emailSender,
    IOptions<PostmarkOptions> postmarkOptions,
    IRateLimiter rateLimiter,
    ILogger<ForgotPasswordCommandHandler> logger) : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IEmailSender<IdentityUser> _emailSender = emailSender;
    private readonly PostmarkOptions _postmarkOptions = postmarkOptions.Value;
    private readonly IRateLimiter _rateLimiter = rateLimiter;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger = logger;

    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return new ForgotPasswordResult(false, Error: "Email is required");

        if (!_rateLimiter.CanProceed($"email_{request.Email}"))
        {
            var remainingSeconds = _rateLimiter.GetRemainingSeconds($"email_{request.Email}");
            return new ForgotPasswordResult(
                false, 
                Error: $"Пожалуйста, подождите {remainingSeconds} секунд перед повторной отправкой письма"
            );
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return new ForgotPasswordResult(true, Message: "Если пользователь существует, письмо отправлено");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        if (string.IsNullOrWhiteSpace(_postmarkOptions.FrontendBaseUrl))
        {
            _logger.LogError("FrontendBaseUrl не настроен в конфигурации Postmark");
            return new ForgotPasswordResult(false, Error: "FrontendBaseUrl is not configured");
        }

        var callbackUrl = $"{_postmarkOptions.FrontendBaseUrl}/resetPassword?email={request.Email}&code={encodedToken}";

        if (request.SendEmail)
        {
            try
            {
                await _emailSender.SendPasswordResetLinkAsync(user, request.Email, callbackUrl);
                
                _rateLimiter.RecordAction($"email_{request.Email}");
                
                return new ForgotPasswordResult(true, Message: "Ссылка для сброса пароля отправлена");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке письма на {Email}", request.Email);
                return new ForgotPasswordResult(false, Error: "Ошибка при отправке письма");
            }
        }

        return new ForgotPasswordResult(true, CallbackUrl: callbackUrl);
    }
}
