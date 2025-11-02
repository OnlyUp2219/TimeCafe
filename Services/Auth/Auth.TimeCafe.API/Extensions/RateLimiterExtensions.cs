using Microsoft.Extensions.Caching.Memory;

using System.Threading.RateLimiting;

namespace Auth.TimeCafe.API.Extensions;

public static class RateLimiterExtensions
{
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

    public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        var minIntervalSeconds = configuration.GetValue<int>("RateLimiter:EmailSms:MinIntervalSeconds");
        var windowMinutes = configuration.GetValue<int>("RateLimiter:EmailSms:WindowMinutes");
        var maxRequests = configuration.GetValue<int>("RateLimiter:EmailSms:MaxRequests");
        var windowSeconds = windowMinutes * 60;

        services.AddSingleton(new RateLimitConfig { MinIntervalSeconds = minIntervalSeconds });

        services.AddRateLimiter(options =>
        {
            options.AddPolicy("OneRequestPerInterval", context =>
            {
                var key = GetRateLimitKey(context);
                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1,
                    Window = TimeSpan.FromSeconds(minIntervalSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });

            options.AddPolicy("MaxRequestPerWindow", context =>
            {
                var key = GetRateLimitKey(context);
                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = maxRequests,
                    Window = TimeSpan.FromSeconds(windowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });

            

            options.RejectionStatusCode = 429;
            options.OnRejected = (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers["Retry-After"] = ((int)retryAfter.TotalSeconds).ToString();
                }
                return ValueTask.CompletedTask;
                
            };
        });

        return services;
    }
}

public class RateLimitConfig
{
    public int MinIntervalSeconds { get; set; }
}
