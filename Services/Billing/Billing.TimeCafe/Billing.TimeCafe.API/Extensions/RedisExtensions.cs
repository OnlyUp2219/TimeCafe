namespace Billing.TimeCafe.API.Extensions;

public static class RedisExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisSection = configuration.GetSection("Redis");
        if (!redisSection.Exists())
            throw new InvalidOperationException("Redis configuration section is missing.");

        var connectionString = redisSection["ConnectionString"]
            ?? throw new InvalidOperationException("Redis:ConnectionString is not configured.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });

        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
        {
            return StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString);
        });

        return services;
    }
}
