namespace Auth.TimeCafe.Application.CQRS.Sender.Commands;

public record class SendSmsCommand(string AccountSid, string AuthToken, string TwilioPhoneNumber, string PhoneNumber, string Token) : IRequest<PhoneVerificationModel?>;

public class SendSmsCommandHendler(ITwilioSender twilioSender, ILogger<SendSmsCommandHendler> logger) : IRequestHandler<SendSmsCommand, PhoneVerificationModel?>
{
    private readonly ITwilioSender _twilioSender = twilioSender;
    private readonly ILogger<SendSmsCommandHendler> _logger = logger;

    public async Task<PhoneVerificationModel?> Handle(SendSmsCommand request, CancellationToken cancellationToken)
    {
        var result = await _twilioSender.SendAsync(
            request.AccountSid,
            request.AuthToken,
            request.TwilioPhoneNumber,
            request.PhoneNumber,
            request.Token);

        if (result == null)
        {
            _logger.LogWarning("Не удалось отправить SMS на номер {PhoneNumber}", request.PhoneNumber);
        }

        return result;
    }
}
