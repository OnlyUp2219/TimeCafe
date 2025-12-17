namespace Auth.TimeCafe.Domain.Contracts;

public interface ITwilioSender
{
    Task<PhoneVerificationResult?> SendAsync(string accountSid, string authToken, string twilioPhoneNumber, string phoneNumber, string token);
}