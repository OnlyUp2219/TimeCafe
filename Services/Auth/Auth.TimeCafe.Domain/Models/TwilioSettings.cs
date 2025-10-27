namespace Auth.TimeCafe.Domain.Models
{
    public class TwilioSettings
    {
        public string? AccountSid { get; set; }
        public string? AuthToken { get; set; }
        public string? VerificationServiceSid { get; set; }
    }
}