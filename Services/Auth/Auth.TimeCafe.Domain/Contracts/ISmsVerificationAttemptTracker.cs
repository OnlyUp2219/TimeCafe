namespace Auth.TimeCafe.Domain.Contracts;

public interface ISmsVerificationAttemptTracker
{
    bool CanVerifyCode(string userId, string phoneNumber);
    void RecordFailedAttempt(string userId, string phoneNumber);
    int GetRemainingAttempts(string userId, string phoneNumber);
    void ResetAttempts(string userId, string phoneNumber);
    void RecordCodeSent(string userId, string phoneNumber);
    bool HasPendingVerification(string userId, string phoneNumber);
    void ClearPendingVerification(string userId, string phoneNumber);
}
