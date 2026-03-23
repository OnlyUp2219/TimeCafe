using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BuildingBlocks.OpenApi;

public static class OpenApiTransformers
{
    public static OpenApiOptions AddBearerSecurityScheme(this OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            var components = document.Components ??= new OpenApiComponents();
            components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
            };
            return Task.CompletedTask;
        });

        return options;
    }

    public static OpenApiOptions AddStandardResponseCodes(this OpenApiOptions options)
    {
        options.AddOperationTransformer((operation, context, _) =>
        {
            var requiresAuth = context.Description.ActionDescriptor.EndpointMetadata
                .Any(m => m is Microsoft.AspNetCore.Authorization.AuthorizeAttribute);

            var allowsAnonymous = context.Description.ActionDescriptor.EndpointMetadata
                .Any(m => m is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute);

            if (requiresAuth && !allowsAnonymous)
            {
                operation.Responses?.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses?.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

                operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer")] = new List<string>()
                    }
                ];
            }

            var httpMethod = context.Description.HttpMethod;
            if (httpMethod is "POST" or "PUT" or "PATCH")
            {
                operation.Responses?.TryAdd("400", new OpenApiResponse { Description = "Validation Error" });
            }

            operation.Responses?.TryAdd("500", new OpenApiResponse { Description = "Internal Server Error" });

            return Task.CompletedTask;
        });

        return options;
    }
}
