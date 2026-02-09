namespace Auth.TimeCafe.API.Extensions;

public static class RateLimiterExtensions
{
    private static string GetRateLimitKey(HttpContext context)
    {
        var endpoint = context.GetEndpoint()?.DisplayName;
        var env = context.RequestServices.GetService<IHostEnvironment>();
        if (env != null
            && (env.IsDevelopment() || env.IsEnvironment("Docker"))
            && context.User.Identity?.IsAuthenticated != true)
        {
            var testKey = context.Request.Headers["X-Test-RateLimit-Key"].ToString();
            if (!string.IsNullOrWhiteSpace(testKey))
            {
                return $"{testKey}:{endpoint}";
            }
        }
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
        var rateLimiterSection = configuration.GetSection("RateLimiter:EmailSms");
        if (!rateLimiterSection.Exists())
            throw new InvalidOperationException("RateLimiter:EmailSms configuration section is missing.");

        var minIntervalSeconds = rateLimiterSection.GetValue<int>("MinIntervalSeconds");
        var windowMinutes = rateLimiterSection.GetValue<int>("WindowMinutes");
        var maxRequests = rateLimiterSection.GetValue<int>("MaxRequests");

        if (minIntervalSeconds <= 0)
            throw new InvalidOperationException("RateLimiter:EmailSms:MinIntervalSeconds must be greater than 0.");
        if (windowMinutes <= 0)
            throw new InvalidOperationException("RateLimiter:EmailSms:WindowMinutes must be greater than 0.");
        if (maxRequests <= 0)
            throw new InvalidOperationException("RateLimiter:EmailSms:MaxRequests must be greater than 0.");

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
