using BuildingBlocks.OpenApi;

namespace Venue.TimeCafe.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new() { Title = "TimeCafe Venue API", Version = "v1" };
                return Task.CompletedTask;
            });

            options.AddBearerSecurityScheme();
            options.AddStandardResponseCodes();
        });

        return services;
    }

    public static WebApplication UseOpenApiDevelopment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options => options
                .WithTitle("TimeCafe Venue API")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithOpenApiRoutePattern("/openapi/{documentName}.json"));
        }

        return app;
    }
}
