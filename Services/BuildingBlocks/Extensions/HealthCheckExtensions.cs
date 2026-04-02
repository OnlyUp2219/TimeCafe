using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using StackExchange.Redis;

namespace BuildingBlocks.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        var pgConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(pgConnectionString))
            hcBuilder.AddCheck("postgresql", new PostgresHealthCheck(pgConnectionString), tags: ["db"]);

        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConnectionString))
            hcBuilder.AddCheck("redis", new RedisHealthCheck(redisConnectionString), tags: ["cache"]);

        return services;
    }

    public static WebApplication UseHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteResponseAsync
        }).AllowAnonymous();

        return app;
    }

    private static async Task WriteResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var result = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                error = e.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsJsonAsync(result);
    }
}

internal sealed class PostgresHealthCheck(string connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT 1";
            await cmd.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}

internal sealed class RedisHealthCheck(string connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var redis = await ConnectionMultiplexer.ConnectAsync(
                ConfigurationOptions.Parse(connectionString));
            var db = redis.GetDatabase();
            await db.PingAsync();
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}
