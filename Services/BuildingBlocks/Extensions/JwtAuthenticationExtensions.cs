using BuildingBlocks.Options;

namespace BuildingBlocks.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<JwtOptions>(configuration, "Jwt");

        services.AddAuthorizationBuilder();
        services.AddTransient<Microsoft.AspNetCore.Authentication.IClaimsTransformation, PermissionClaimsEnrichmentTransformer>();

        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

#if DEBUG
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrWhiteSpace(context.Token))
                        {
                            var cookieToken = context.Request.Cookies["Access-Token"];
                            if (!string.IsNullOrWhiteSpace(cookieToken))
                            {
                                context.Token = cookieToken;
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
#endif
            });

        return services;
    }

    public static ClaimsPrincipal? TryGetCurrentUser(this HttpContext httpContext) => httpContext.User;
}
