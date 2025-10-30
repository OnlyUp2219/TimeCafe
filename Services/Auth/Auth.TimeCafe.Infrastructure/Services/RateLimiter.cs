namespace Auth.TimeCafe.Infrastructure.Services;

public class RateLimiter(IMemoryCache cache, IConfiguration configuration) : IRateLimiter
{
    private readonly IMemoryCache _cache = cache;
    private readonly TimeSpan _throttleTime = TimeSpan.FromSeconds(
        configuration.GetValue<int>("RateLimiting:ThrottleSeconds", 60)
    );

    public bool CanProceed(string identifier)
    {
        var cacheKey = GetCacheKey(identifier);
        return !_cache.TryGetValue(cacheKey, out _);
    }

    public void RecordAction(string identifier)
    {
        var cacheKey = GetCacheKey(identifier);
        _cache.Set(cacheKey, DateTime.UtcNow, _throttleTime);
    }

    public int GetRemainingSeconds(string identifier)
    {
        var cacheKey = GetCacheKey(identifier);
        
        if (!_cache.TryGetValue(cacheKey, out DateTime recordedTime))
        {
            return 0;
        }

        var elapsed = DateTime.UtcNow - recordedTime;
        var remaining = _throttleTime - elapsed;
        
        return remaining.TotalSeconds > 0 
            ? (int)Math.Ceiling(remaining.TotalSeconds) 
            : 0;
    }

    private static string GetCacheKey(string identifier)
    {
        return $"rate_limit_{identifier}";
    }
}
