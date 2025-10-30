namespace Auth.TimeCafe.Domain.Contracts;

public interface IRateLimiter
{
    bool CanProceed(string identifier);
    void RecordAction(string identifier);
    int GetRemainingSeconds(string identifier);
}
