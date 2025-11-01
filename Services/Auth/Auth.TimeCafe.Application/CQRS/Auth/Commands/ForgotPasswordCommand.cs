namespace Auth.TimeCafe.Application.CQRS.Auth.Commands;

public record ForgotPasswordCommand(string Email, bool SendEmail = true) : IRequest<Result<ForgotPasswordResponse>>;

public record ForgotPasswordResponse(string? CallbackUrl = null, string? Message = null);

public class ForgotPasswordCommandHandler(
    UserManager<IdentityUser> userManager,
    IEmailSender<IdentityUser> emailSender,
    IOptions<PostmarkOptions> postmarkOptions,
    IRateLimiter rateLimiter) : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResponse>>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IEmailSender<IdentityUser> _emailSender = emailSender;
    private readonly PostmarkOptions _postmarkOptions = postmarkOptions.Value;
    private readonly IRateLimiter _rateLimiter = rateLimiter;

    public async Task<Result<ForgotPasswordResponse>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<ForgotPasswordResponse>.ValidationError("email", "Email обязателен");

        if (!_rateLimiter.CanProceed($"email_{request.Email}"))
        {
            var remainingSeconds = _rateLimiter.GetRemainingSeconds($"email_{request.Email}");
            return Result<ForgotPasswordResponse>.RateLimit(
                $"Пожалуйста, подождите {remainingSeconds} секунд перед повторной отправкой письма",
                remainingSeconds
            );
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Унифицированное сообщение для предотвращения перечисления пользователей
            return Result<ForgotPasswordResponse>.Success(
                new ForgotPasswordResponse(Message: "Если пользователь существует, письмо отправлено")
            );
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        if (string.IsNullOrWhiteSpace(_postmarkOptions.FrontendBaseUrl))
        {
            return Result<ForgotPasswordResponse>.Critical("Конфигурация FrontendBaseUrl не настроена");
        }

        var callbackUrl = $"{_postmarkOptions.FrontendBaseUrl}/resetPassword?email={request.Email}&code={encodedToken}";

        if (request.SendEmail)
        {
            try
            {
                await _emailSender.SendPasswordResetLinkAsync(user, request.Email, callbackUrl);
                _rateLimiter.RecordAction($"email_{request.Email}");

                return Result<ForgotPasswordResponse>.Success(
                    new ForgotPasswordResponse(Message: "Ссылка для сброса пароля отправлена")
                );
            }
            catch (Exception)
            {
                return Result<ForgotPasswordResponse>.Critical("Ошибка при отправке письма");
            }
        }

        return Result<ForgotPasswordResponse>.Success(
            new ForgotPasswordResponse(CallbackUrl: callbackUrl, Message: "Ссылка сгенерирована")
        );
    }
}
