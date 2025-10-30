using Auth.TimeCafe.Domain.Contracts;

using Microsoft.Extensions.Caching.Memory;

namespace Auth.TimeCafe.Infrastructure.Services.Phone;

public class SmsRateLimiter(IMemoryCache cache) : ISmsRateLimiter
{
    private readonly IMemoryCache _cache = cache;
    private readonly TimeSpan _throttleTime = TimeSpan.FromMinutes(1);

    public bool CanSendSms(string userId)
    {
        var cacheKey = $"sms_rate_limit_{userId}";
        return !_cache.TryGetValue(cacheKey, out _);
    }

    public void RecordSmsSent(string userId)
    {
        var cacheKey = $"sms_rate_limit_{userId}";
        _cache.Set(cacheKey, true, _throttleTime);
    }
}
