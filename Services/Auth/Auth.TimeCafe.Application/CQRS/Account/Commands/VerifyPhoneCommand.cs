namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record VerifyPhoneCommand(
    string UserId,
    string PhoneNumber,
    string Code,
    string? CaptchaToken,
    bool Mock = false
) : IRequest<VerifyPhoneResult>;

public record VerifyPhoneResult(
    bool Success,
    string? Message = null,
    List<string>? Errors = null,
    ETypeError? TypeError = null,
    string? PhoneNumber = null,
    int? RemainingAttempts = null,
    bool? RequiresCaptcha = null
) : ICqrsResult;

public class VerifyPhoneCommandHandler(
    UserManager<IdentityUser> userManager,
    ISmsVerificationAttemptTracker attemptTracker,
    ICaptchaValidator captchaValidator
) : IRequestHandler<VerifyPhoneCommand, VerifyPhoneResult>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly ISmsVerificationAttemptTracker _attemptTracker = attemptTracker;
    private readonly ICaptchaValidator _captchaValidator = captchaValidator;

    public async Task<VerifyPhoneResult> Handle(VerifyPhoneCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new VerifyPhoneResult(false, Message: "Пользователь не найден", Errors: ["UserNotFound"], TypeError: ETypeError.BadRequest, PhoneNumber: request.PhoneNumber, RemainingAttempts: 0, RequiresCaptcha: false);

        if (!_attemptTracker.CanVerifyCode(request.UserId, request.PhoneNumber))
            return new VerifyPhoneResult(false, Message: "Превышено количество попыток. Запросите новый код.", Errors: ["TooManyAttempts"], TypeError: ETypeError.BadRequest, PhoneNumber: request.PhoneNumber, RemainingAttempts: 0, RequiresCaptcha: false);

        var remaining = _attemptTracker.GetRemainingAttempts(request.UserId, request.PhoneNumber);
        if (remaining == 3)
        {
            if (string.IsNullOrEmpty(request.CaptchaToken))
                return new VerifyPhoneResult(false, Message: "Пройдите проверку капчи", Errors: ["CaptchaRequired"], TypeError: ETypeError.BadRequest, PhoneNumber: request.PhoneNumber, RemainingAttempts: remaining, RequiresCaptcha: true);

            if (!await _captchaValidator.ValidateAsync(request.CaptchaToken))
                return new VerifyPhoneResult(false, Message: "Пройдите проверку капчи", Errors: ["CaptchaInvalid"], TypeError: ETypeError.BadRequest, PhoneNumber: request.PhoneNumber, RemainingAttempts: remaining, RequiresCaptcha: true);
        }

        var identityResult = await _userManager.ChangePhoneNumberAsync(user, request.PhoneNumber, request.Code);
        if (identityResult.Succeeded)
        {
            _attemptTracker.ResetAttempts(request.UserId, request.PhoneNumber);
            return new VerifyPhoneResult(true, Message: request.Mock ? "Номер телефона успешно подтвержден (mock)" : "Номер телефона успешно подтвержден", PhoneNumber: request.PhoneNumber, RemainingAttempts: _attemptTracker.GetRemainingAttempts(request.UserId, request.PhoneNumber), RequiresCaptcha: false);
        }

        _attemptTracker.RecordFailedAttempt(request.UserId, request.PhoneNumber);
        remaining = _attemptTracker.GetRemainingAttempts(request.UserId, request.PhoneNumber);
        return new VerifyPhoneResult(false, Message: "Неверный код подтверждения или истек срок действия", Errors: ["InvalidCode"], TypeError: ETypeError.BadRequest, PhoneNumber: request.PhoneNumber, RemainingAttempts: remaining, RequiresCaptcha: remaining == 3);
    }
}
