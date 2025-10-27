using Auth.TimeCafe.Domain.Models;
using Auth.TimeCafe.Domain.Contracts;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Auth.TimeCafe.Application.CQRS.Sender.Queries;

public record class SendCommand(string AccountSid, string AuthToken, string TwilioPhoneNumber, string PhoneNumber, string Token) : IRequest<PhoneVerificationModel?>;

public class SendCommandHendler(ITwilioSender twilioSender, ILogger<SendCommandHendler> logger) : IRequestHandler<SendCommand, PhoneVerificationModel?>
{
    private readonly ITwilioSender _twilioSender = twilioSender;
    private readonly ILogger<SendCommandHendler> _logger = logger;
    public async Task<PhoneVerificationModel?> Handle(SendCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Отправка SMS с кодом подтверждения для номера {PhoneNumber}", request.PhoneNumber);
        
        var result = await _twilioSender.SendAsync(
            request.AccountSid,
            request.AuthToken,
            request.TwilioPhoneNumber,
            request.PhoneNumber,
            request.Token);

        if (result != null)
        {
            _logger.LogInformation("SMS успешно отправлено на номер {PhoneNumber}", request.PhoneNumber);
        }
        else
        {
            _logger.LogWarning("Не удалось отправить SMS на номер {PhoneNumber}", request.PhoneNumber);
        }

        return result;
    }
}
