namespace Auth.TimeCafe.Domain.Contracts;

public interface ISmsRateLimiter
{
    bool CanSendSms(string userId);
    void RecordSmsSent(string userId);
}
