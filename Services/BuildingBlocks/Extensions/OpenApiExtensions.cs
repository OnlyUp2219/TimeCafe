using BuildingBlocks.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace BuildingBlocks.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services, string title)
    {
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new() { Title = title, Version = "v1" };
                return Task.CompletedTask;
            });

            options.AddBearerSecurityScheme();
            options.AddStandardResponseCodes();
        });

        return services;
    }

    public static WebApplication UseOpenApiDevelopment(this WebApplication app, string title)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options => options
                .WithTitle(title)
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithOpenApiRoutePattern("/openapi/{documentName}.json"));
        }

        return app;
    }
}
