namespace YarpProxy.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddSpaCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Spa", policy =>
            {
                policy
                    .WithOrigins(
                        "http://127.0.0.1:9301",
                        "http://localhost:9301")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static WebApplication UseSpaCorsConfiguration(this WebApplication app)
    {
        app.UseCors("Spa");
        return app;
    }
}
