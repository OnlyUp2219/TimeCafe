using Auth.TimeCafe.Domain.Contracts;

using Microsoft.Extensions.Logging;

using Twilio;
using Twilio.Rest.Api.V2010.Account;


namespace Auth.TimeCafe.Infrastructure.Services;

public class TwilioSender(ILogger<TwilioSender> logger) : ITwilioSender
{
    private readonly ILogger<TwilioSender> _logger = logger;
    public async Task<PhoneVerificationModel?> SendAsync(string accountSid, string authToken, string twilioPhoneNumber, string phoneNumber, string token)
    {
        TwilioClient.Init(accountSid, authToken);

        var message = await MessageResource.CreateAsync(
            body: $"Ваш код подтверждения: {token}",
            from: new Twilio.Types.PhoneNumber(twilioPhoneNumber),
            to: new Twilio.Types.PhoneNumber(phoneNumber)
        );

        if (message.ErrorCode == null)
        {
            return new PhoneVerificationModel
            {
                PhoneNumber = phoneNumber,
                Code = string.Empty
            };
        }

        _logger.LogError("Twilio ошибка при отправке SMS на {PhoneNumber}: ErrorCode={ErrorCode}, ErrorMessage={ErrorMessage}",
            phoneNumber, message.ErrorCode, message.ErrorMessage);

        return null;
    }
}
