using Twilio;
using Twilio.Rest.Api.V2010.Account;


namespace Auth.TimeCafe.Infrastructure.Services.Phone;

public class TwilioSender(ILogger<TwilioSender> logger) : ITwilioSender
{
    private readonly ILogger<TwilioSender> _logger = logger;
    public async Task<PhoneVerificationResult?> SendAsync(string accountSid, string authToken, string twilioPhoneNumber, string phoneNumber, string token)
    {
        try
        {
            TwilioClient.Init(accountSid, authToken);

            var message = await MessageResource.CreateAsync(
                body: $"Ваш код подтверждения: {token}",
                from: new Twilio.Types.PhoneNumber(twilioPhoneNumber),
                to: new Twilio.Types.PhoneNumber(phoneNumber)
            );

            if (message.ErrorCode == null)
            {
                return new PhoneVerificationResult
                {
                    PhoneNumber = phoneNumber,
                    Success = true
                };
            }

            _logger.LogError("Twilio ошибка при отправке SMS на {PhoneNumber}: ErrorCode={ErrorCode}, ErrorMessage={ErrorMessage}",
                phoneNumber, message.ErrorCode, message.ErrorMessage);

            return null;
        }
        catch (Twilio.Exceptions.ApiException ex)
        {
            _logger.LogError(ex, "Twilio API ошибка при отправке SMS на {PhoneNumber}: {Message}", phoneNumber, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при отправке SMS на {PhoneNumber}", phoneNumber);
            return null;
        }
    }
}
