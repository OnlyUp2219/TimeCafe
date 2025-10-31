namespace Auth.TimeCafe.Application.CQRS.Sender.Commands;

public record SendSmsCommand(
    string AccountSid,
    string AuthToken,
    string TwilioPhoneNumber,
    string PhoneNumber,
    string Token) : IRequest<Result<PhoneVerificationModel>>;

public class SendSmsCommandHandler(
    ITwilioSender twilioSender,
    ILogger<SendSmsCommandHandler> logger) : IRequestHandler<SendSmsCommand, Result<PhoneVerificationModel>>
{
    private readonly ITwilioSender _twilioSender = twilioSender;
    private readonly ILogger<SendSmsCommandHandler> _logger = logger;

    public async Task<Result<PhoneVerificationModel>> Handle(SendSmsCommand request, CancellationToken cancellationToken)
    {
        var result = await _twilioSender.SendAsync(
            request.AccountSid,
            request.AuthToken,
            request.TwilioPhoneNumber,
            request.PhoneNumber,
            request.Token);
        //Todo rate limiter
        if (result == null)
        {
            _logger.LogWarning("Не удалось отправить SMS на номер {PhoneNumber}", request.PhoneNumber);
            return Result<PhoneVerificationModel>.Failure("Не удалось отправить SMS");
        }

        return Result<PhoneVerificationModel>.Success(result);
    }
}
