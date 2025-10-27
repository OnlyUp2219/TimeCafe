using Auth.TimeCafe.Domain.Models;

using System.Threading.Tasks;

namespace Auth.TimeCafe.Domain.Contracts;

public interface ITwilioSender
{
    Task<PhoneVerificationModel?> SendAsync(string accountSid, string authToken, string twilioPhoneNumber, string phoneNumber, string token);
}