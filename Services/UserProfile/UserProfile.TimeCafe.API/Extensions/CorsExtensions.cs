namespace UserProfile.TimeCafe.API.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, string? corsPolicyName)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(corsPolicyName ?? "", p =>
                p.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Retry-After", "X-Rate-Limit-Window", "X-Rate-Limit-Remaining")
                .WithOrigins(
                "http://127.0.0.1:9301",
                "http://localhost:9301",
                "http://127.0.0.1:4173",
                "http://localhost:4173",
                "http://127.0.0.1:5173",
                "http://localhost:5173",
                "http://127.0.0.1:8001",  
                "http://localhost:8001",
                "http://127.0.0.1:8002", 
                "http://localhost:8002",
                "https://127.0.0.1:9301",
                "https://localhost:9301",
                "https://127.0.0.1:4173",
                "https://localhost:4173",
                "https://127.0.0.1:5173",
                "https://localhost:5173"));
        });

        return services;
    }
}
