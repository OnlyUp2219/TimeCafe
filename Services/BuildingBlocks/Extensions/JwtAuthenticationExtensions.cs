namespace BuildingBlocks.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorizationBuilder();
        services.AddTransient<Microsoft.AspNetCore.Authentication.IClaimsTransformation, PermissionClaimsEnrichmentTransformer>();

        var jwtSection = configuration.GetSection("Jwt");
        if (!jwtSection.Exists())
            throw new InvalidOperationException("Jwt configuration section is missing.");

        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
        var signingKey = jwtSection["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
        var keyBytes = Encoding.UTF8.GetBytes(signingKey);

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
                    ValidIssuer = issuer,
                    ValidAudience = audience,
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
