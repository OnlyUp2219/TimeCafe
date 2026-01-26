namespace YarpProxy.Extensions;

public static class ScalarExtensions
{
    public static IServiceCollection AddScalarConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi();

        services.AddHttpClient("openapi")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

        return services;
    }

    public static WebApplication UseScalarConfiguration(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapScalarApiReference(options =>
        {
            options.WithTitle("TimeCafe Gateway")
                   .WithTheme(ScalarTheme.DeepSpace)
                   .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                   .WithOpenApiRoutePattern("/openapi/{documentName}.json");

                 options.AddDocument(
                documentName: "auth",
                title: "TimeCafe Auth API",
                routePattern: "/openapi/auth.json",
                isDefault: true);
        });

        return app;
    }
}
