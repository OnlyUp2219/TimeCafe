namespace Auth.TimeCafe.API.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration, string? corsPolicyName)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(corsPolicyName, p =>
                p.AllowAnyHeader().
                AllowAnyMethod().
                AllowCredentials().
                WithOrigins("http://127.0.0.1:9301",
                "http://localhost:9301",
                "http://127.0.0.1:4173",
                "http://localhost:4173"));
        });

        return services;
    }
}
