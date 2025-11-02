using Microsoft.Extensions.Caching.Memory;

namespace Auth.TimeCafe.API.Middleware;

public class RateLimitCounterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;

    public RateLimitCounterMiddleware(RequestDelegate next, IMemoryCache cache, IConfiguration configuration)
    {
        _next = next;
        _cache = cache;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var key = GetRateLimitKey(context);
        var windowMinutes = _configuration.GetValue<int>("RateLimiter:EmailSms:WindowMinutes");
        var maxRequests = _configuration.GetValue<int>("RateLimiter:EmailSms:MaxRequests");
        var windowSeconds = windowMinutes * 60;

        var countKey = $"{key}:count";
        var windowKey = $"{key}:window_start";

        if (!_cache.TryGetValue(windowKey, out DateTimeOffset windowStart))
        {
            windowStart = DateTimeOffset.UtcNow;
            _cache.Set(windowKey, windowStart, TimeSpan.FromSeconds(windowSeconds));
        }

        var count = _cache.GetOrCreate(countKey, entry =>
        {
            entry.AbsoluteExpiration = windowStart.AddSeconds(windowSeconds);
            return 0;
        });

        context.Response.OnStarting(() =>
        {
            if (context.Response.StatusCode != 429)
            {
                count++;
                _cache.Set(countKey, count, windowStart.AddSeconds(windowSeconds));
                var remaining = Math.Max(0, maxRequests - count);
                context.Response.Headers["X-Rate-Limit-Remaining"] = remaining.ToString();
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static string GetRateLimitKey(HttpContext context)
    {
        var endpoint = context.GetEndpoint()?.DisplayName;
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.Identity.Name ?? "unknown";
            return $"{userId}:{endpoint}";
        }
        else
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anon";
            return $"{ip}:{endpoint}";
        }
    }
}
