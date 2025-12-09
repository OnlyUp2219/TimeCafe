using Auth.TimeCafe.Application.CQRS.Sender.Commands;

using Microsoft.Extensions.Configuration;


namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record GeneratePhoneVerificationCommand(string UserId, string PhoneNumber, bool Mock = false) : IRequest<GeneratePhoneVerificationResult>;

public record GeneratePhoneVerificationResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    string? CallbackUrl = null,
    string? PhoneNumber = null,
    string? Token = null) : ICqrsResultV2
{
    public static GeneratePhoneVerificationResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 401);

    public static GeneratePhoneVerificationResult MockCallback(string phoneNumber, string token) =>
        new(true, Message: "Mock SMS сгенерировано", PhoneNumber: phoneNumber, Token: token);

    public static GeneratePhoneVerificationResult Sent(string PhoneNumber) =>
    new(true, Message: "SMS отправлено",
        PhoneNumber: PhoneNumber);

    public static GeneratePhoneVerificationResult SendFailed(string PhoneNumber) =>
        new(false, Code: "SendFailed", Message: "Ошибка при отправке письма", StatusCode: 500,
            PhoneNumber: PhoneNumber);
}

public class GeneratePhoneVerificationCommandValidator : AbstractValidator<GeneratePhoneVerificationCommand>
{
    public GeneratePhoneVerificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Пользователь не найден");
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Номер телефона не может быть пустым")
            .Matches(@"^\+\d{10,15}$").WithMessage("Неверный формат номера телефона. Используйте формат +12345678901");
    }
}

public class GeneratePhoneVerificationCommandHandler(
UserManager<ApplicationUser> userManager,
IConfiguration configuration,
ISender sender,
ISmsVerificationAttemptTracker attemptTracker
) : IRequestHandler<GeneratePhoneVerificationCommand, GeneratePhoneVerificationResult>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly ISender _sender = sender;
    private readonly ISmsVerificationAttemptTracker _attemptTracker = attemptTracker;

    public async Task<GeneratePhoneVerificationResult> Handle(GeneratePhoneVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return GeneratePhoneVerificationResult.UserNotFound();

        var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, request.PhoneNumber);

        if (request.Mock)
        {
            _attemptTracker.ResetAttempts(request.UserId, request.PhoneNumber);
            return GeneratePhoneVerificationResult.MockCallback(request.PhoneNumber, token);
        }

        string accountSid = _configuration["Twilio:AccountSid"] ?? string.Empty;
        string authToken = _configuration["Twilio:AuthToken"] ?? string.Empty;
        string twilioPhoneNumber = _configuration["Twilio:TwilioPhoneNumber"] ?? string.Empty;

        var sendSmsCommand = new SendSmsCommand(accountSid, authToken, twilioPhoneNumber, request.PhoneNumber, token);
        var sendResult = await _sender.Send(sendSmsCommand, cancellationToken);

        if (sendResult != null)
        {
            _attemptTracker.ResetAttempts(request.UserId, request.PhoneNumber);
            return GeneratePhoneVerificationResult.Sent(request.PhoneNumber);
        }

        return GeneratePhoneVerificationResult.SendFailed(request.PhoneNumber);
    }
}
