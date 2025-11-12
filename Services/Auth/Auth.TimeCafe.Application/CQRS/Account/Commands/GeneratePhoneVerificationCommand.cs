using Auth.TimeCafe.Application.CQRS.Sender.Commands;
using Microsoft.Extensions.Configuration;


namespace Auth.TimeCafe.Application.CQRS.Account.Commands;

public record GeneratePhoneVerificationCommand(string UserId, string PhoneNumber, bool Mock = false) : IRequest<GeneratePhoneVerificationResult>;

public record GeneratePhoneVerificationResult(
    bool Success,
    string? Message = null,
    List<string>? Errors = null,
    ETypeError? TypeError = null,
    string? PhoneNumber = null,
    string? Token = null
) : ICqrsResult;

public class GeneratePhoneVerificationCommandHandler(
    UserManager<IdentityUser> userManager,
    IConfiguration configuration,
    IMediator mediator,
    ISmsVerificationAttemptTracker attemptTracker
) : IRequestHandler<GeneratePhoneVerificationCommand, GeneratePhoneVerificationResult>
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly IMediator _mediator = mediator;
    private readonly ISmsVerificationAttemptTracker _attemptTracker = attemptTracker;

    public async Task<GeneratePhoneVerificationResult> Handle(GeneratePhoneVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new GeneratePhoneVerificationResult(false, Message: "Пользователь не найден", Errors: ["UserNotFound"], TypeError: ETypeError.BadRequest);

        var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, request.PhoneNumber);

        if (request.Mock)
        {
            _attemptTracker.ResetAttempts(request.UserId, request.PhoneNumber);
            return new GeneratePhoneVerificationResult(true, Message: "SMS отправлено (mock)", PhoneNumber: request.PhoneNumber, Token: token);
        }

        string accountSid = _configuration["Twilio:AccountSid"] ?? string.Empty;
        string authToken = _configuration["Twilio:AuthToken"] ?? string.Empty;
        string twilioPhoneNumber = _configuration["Twilio:TwilioPhoneNumber"] ?? string.Empty;

        var sendSmsCommand = new SendSmsCommand(accountSid, authToken, twilioPhoneNumber, request.PhoneNumber, token);
        var sendResult = await _mediator.Send(sendSmsCommand, cancellationToken);

        if (sendResult != null)
        {
            _attemptTracker.ResetAttempts(request.UserId, request.PhoneNumber);
            return new GeneratePhoneVerificationResult(true, Message: "SMS отправлено", PhoneNumber: sendResult.PhoneNumber);
        }

        return new GeneratePhoneVerificationResult(false, Message: "Ошибка при отправке SMS", PhoneNumber: request.PhoneNumber, Errors: ["SendFailed"], TypeError: ETypeError.External);
    }
}
