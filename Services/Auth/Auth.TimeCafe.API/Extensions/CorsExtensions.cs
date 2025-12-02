namespace Auth.TimeCafe.API.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, string? corsPolicyName)
    {
        if (string.IsNullOrWhiteSpace(corsPolicyName))
            throw new InvalidOperationException("CORS:PolicyName is not configured.");

        services.AddCors(options =>
        {
            options.AddPolicy(corsPolicyName, p =>
                p.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Retry-After", "X-Rate-Limit-Window", "X-Rate-Limit-Remaining")
                .WithOrigins(
                    // Local development
                    "http://127.0.0.1:9301",
                    "http://localhost:9301",
                    "http://127.0.0.1:4173",
                    "http://localhost:4173",
                    "http://127.0.0.1:5173",
                    "http://localhost:5173",
                    "https://127.0.0.1:9301",
                    "https://localhost:9301",
                    "https://127.0.0.1:4173",
                    "https://localhost:4173",
                    "https://127.0.0.1:5173",
                    "https://localhost:5173",
                    // Docker services (port 8001 - Auth API)
                    "http://127.0.0.1:8001",
                    "http://localhost:8001",
                    "http://auth-api:8001",
                    // Docker services (port 8002 - UserProfile API)
                    "http://127.0.0.1:8002",
                    "http://localhost:8002",
                    "http://userprofile-api:8002",
                    // Docker services (port 8003 - Venue API)
                    "http://127.0.0.1:8003",
                    "http://localhost:8003",
                    "http://venue-api:8003",
                    // Docker services (port 8004 - Main API)
                    "http://127.0.0.1:8004",
                    "http://localhost:8004",
                    "http://main-api:8004"
                ));
        });

        return services;
    }
}
