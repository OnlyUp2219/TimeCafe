namespace Auth.TimeCafe.API.Extensions;

public static class ScalarExtensions
{
    public static WebApplication UseScalarConfiguration(this WebApplication app)
    {
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("TimeCafe Auth API")
                   .WithTheme(ScalarTheme.DeepSpace)
                   .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                   .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");
        });

        return app;
    }
}
