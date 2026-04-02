namespace Auth.TimeCafe.Infrastructure.Services.Phone;

public class SmsVerificationAttemptTracker(IMemoryCache cache) : ISmsVerificationAttemptTracker
{
    private readonly IMemoryCache _cache = cache;
    private const int MaxAttempts = 5;
    private const int AttemptWindowMinutes = 10;

    public bool CanVerifyCode(string userId, string phoneNumber)
    {
        var attempts = GetCurrentAttempts(userId, phoneNumber);
        return attempts < MaxAttempts;
    }

    public void RecordFailedAttempt(string userId, string phoneNumber)
    {
        var key = GetCacheKey(userId, phoneNumber);
        var attempts = GetCurrentAttempts(userId, phoneNumber);

        _cache.Set(key, attempts + 1, TimeSpan.FromMinutes(AttemptWindowMinutes));
    }

    public int GetRemainingAttempts(string userId, string phoneNumber)
    {
        var attempts = GetCurrentAttempts(userId, phoneNumber);
        return Math.Max(0, MaxAttempts - attempts);
    }

    public void ResetAttempts(string userId, string phoneNumber)
    {
        var key = GetCacheKey(userId, phoneNumber);
        _cache.Remove(key);
    }

    public void RecordCodeSent(string userId, string phoneNumber)
    {
        var key = $"sms_code_sent:{userId}:{phoneNumber}";
        _cache.Set(key, true, TimeSpan.FromMinutes(AttemptWindowMinutes));
    }

    public bool HasPendingVerification(string userId, string phoneNumber)
    {
        var key = $"sms_code_sent:{userId}:{phoneNumber}";
        return _cache.TryGetValue(key, out bool _);
    }

    public void ClearPendingVerification(string userId, string phoneNumber)
    {
        var key = $"sms_code_sent:{userId}:{phoneNumber}";
        _cache.Remove(key);
    }

    private int GetCurrentAttempts(string userId, string phoneNumber)
    {
        var key = GetCacheKey(userId, phoneNumber);
        return _cache.TryGetValue(key, out int attempts) ? attempts : 0;
    }

    private static string GetCacheKey(string userId, string phoneNumber)
        => $"sms_verify_attempts:{userId}:{phoneNumber}";
}
