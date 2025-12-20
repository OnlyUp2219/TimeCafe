namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record VerifyPhoneCommand(string UserId, string PhoneNumber, string Code, string? CaptchaToken, bool Mock = false) : IRequest<VerifyPhoneResult>;

public record VerifyPhoneResult(
            bool Success,
            string? Code = null,
            string? Message = null,
            int? StatusCode = null,
            List<ErrorItem>? Errors = null,
            string? PhoneNumber = null,
            int? RemainingAttempts = null,
            bool? RequiresCaptcha = null
        ) : ICqrsResultV2
{
    public static VerifyPhoneResult UserNotFound(string phoneNumber, int remainingAttempts, bool requiresCaptcha) =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 401,
            PhoneNumber: phoneNumber, RemainingAttempts: remainingAttempts, RequiresCaptcha: requiresCaptcha);

    public static VerifyPhoneResult TooManyAttempts(string phoneNumber, int remainingAttempts, bool requiresCaptcha) =>
        new(false, Code: "TooManyAttempts", Message: "Превышено количество попыток. Запросите новый код.", StatusCode: 429,
            PhoneNumber: phoneNumber, RemainingAttempts: remainingAttempts, RequiresCaptcha: requiresCaptcha);

    public static VerifyPhoneResult CaptchaRequired(string phoneNumber, int remainingAttempts, bool requiresCaptcha) =>
        new(false, Code: "CaptchaRequired", Message: "Пройдите проверку капчи", StatusCode: 400,
            PhoneNumber: phoneNumber, RemainingAttempts: remainingAttempts, RequiresCaptcha: requiresCaptcha);

    public static VerifyPhoneResult CaptchaInvalid(string phoneNumber, int remainingAttempts, bool requiresCaptcha) =>
        new(false, Code: "CaptchaInvalid", Message: "Неверная капча", StatusCode: 400,
            PhoneNumber: phoneNumber, RemainingAttempts: remainingAttempts, RequiresCaptcha: requiresCaptcha);

    public static VerifyPhoneResult SuccessResult(string phoneNumber, int remainingAttempts, bool requiresCaptcha, bool mock) =>
    new(true,
        Message: mock ? "Номер телефона успешно подтвержден (mock)" : "Номер телефона успешно подтвержден",
        PhoneNumber: phoneNumber, RemainingAttempts: remainingAttempts, RequiresCaptcha: requiresCaptcha);

    public static VerifyPhoneResult InvalidCode(string phoneNumber, int remainingAttempts, bool requiresCaptcha) =>
        new(false, Code: "InvalidCode", Message: "Неверный код подтверждения или истек срок действия", StatusCode: 400,
            Errors: [new ErrorItem("InvalidCode", "Код недействителен")],
            PhoneNumber: phoneNumber, RemainingAttempts: remainingAttempts, RequiresCaptcha: requiresCaptcha);
}

public class VerifyPhoneCommandValidator : AbstractValidator<VerifyPhoneCommand>
{
    public VerifyPhoneCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
           .NotNull().WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Номер телефона не может быть пустым")
            .Matches(@"^\+\d{10,15}$").WithMessage("Неверный формат номера телефона");
    }
}

public class VerifyPhoneCommandHandler(
    UserManager<ApplicationUser> userManager,
    ISmsVerificationAttemptTracker attemptTracker,
    ICaptchaValidator captchaValidator
) : IRequestHandler<VerifyPhoneCommand, VerifyPhoneResult>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ISmsVerificationAttemptTracker _attemptTracker = attemptTracker;
    private readonly ICaptchaValidator _captchaValidator = captchaValidator;

    public async Task<VerifyPhoneResult> Handle(VerifyPhoneCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return VerifyPhoneResult.UserNotFound(request.PhoneNumber, 0, false);
        if (!_attemptTracker.CanVerifyCode(request.UserId, request.PhoneNumber))
            return VerifyPhoneResult.TooManyAttempts(request.PhoneNumber, 0, requiresCaptcha: true);

        var remaining = _attemptTracker.GetRemainingAttempts(userId: request.UserId, request.PhoneNumber);
        if (remaining == 3)
        {
            if (string.IsNullOrEmpty(request.CaptchaToken))
                return VerifyPhoneResult.CaptchaRequired(request.PhoneNumber, remainingAttempts: remaining, requiresCaptcha: true);

            if (!await _captchaValidator.ValidateAsync(request.CaptchaToken))
                return VerifyPhoneResult.CaptchaInvalid(request.PhoneNumber, remainingAttempts: remaining, requiresCaptcha: true);
        }

        var identityResult = await _userManager.ChangePhoneNumberAsync(user, request.PhoneNumber, request.Code);
        if (identityResult.Succeeded)
        {
            _attemptTracker.ResetAttempts(request.UserId, request.PhoneNumber);
            var attempts = _attemptTracker.GetRemainingAttempts(request.UserId, request.PhoneNumber);

            return VerifyPhoneResult.SuccessResult(request.PhoneNumber, attempts, false, request.Mock);
        }

        _attemptTracker.RecordFailedAttempt(request.UserId, request.PhoneNumber);
        remaining = _attemptTracker.GetRemainingAttempts(request.UserId, request.PhoneNumber);

        if (remaining <= 0)
        {
            return VerifyPhoneResult.TooManyAttempts(request.PhoneNumber, remaining, requiresCaptcha: true);
        }

        return VerifyPhoneResult.InvalidCode(request.PhoneNumber, remaining, remaining == 3);
    }
}
