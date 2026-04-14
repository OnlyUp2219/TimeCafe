namespace BuildingBlocks.Extensions;

public static class CorsExtensions
{
    public static string AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var policyName = configuration["CORS:PolicyName"]
            ?? throw new InvalidOperationException("CORS:PolicyName is not configured.");

        var origins = configuration.GetSection("CORS:AllowedOrigins").Get<string[]>()
            ?? throw new InvalidOperationException("CORS:AllowedOrigins is not configured.");

        services.AddCors(options => options.AddPolicy(policyName, p =>
            p.AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials()
             .WithExposedHeaders("Retry-After", "X-Rate-Limit-Window", "X-Rate-Limit-Remaining")
             .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
             .WithOrigins(origins)));

        return policyName;
    }
}
