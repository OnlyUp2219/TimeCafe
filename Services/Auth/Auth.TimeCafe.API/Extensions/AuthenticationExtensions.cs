using Microsoft.Extensions.Caching.Hybrid;

namespace Auth.TimeCafe.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddTransient<IClaimsTransformation, PermissionClaimsTransformer>();
        services.AddAuthorizationBuilder();

        ConfigurePermissionCaching(services, configuration, environment);

        var jwtSection = configuration.GetSection("Jwt");
        if (!jwtSection.Exists())
            throw new InvalidOperationException("Jwt configuration section is missing.");

        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
        var signingKey = jwtSection["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
        var keyBytes = Encoding.UTF8.GetBytes(signingKey);

        var authBuilder = services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "sub",
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
                        context.Token = context.Request.Cookies["Access-Token"];
                        return Task.CompletedTask;
                    }
                };
#endif

            })
            .AddCookie(IdentityConstants.ExternalScheme);

        var google = configuration.GetSection("Authentication:Google");
        if (!string.IsNullOrWhiteSpace(google["ClientId"]))
        {
            authBuilder.AddGoogle(op =>
            {
                op.ClientId = google["ClientId"]!;
                op.ClientSecret = google["ClientSecret"] ?? string.Empty;
                op.CallbackPath = "/auth/signin-google";
                op.Events.OnRemoteFailure = context =>
                {
                    var returnUrl = context.Request.Query["returnUrl"].FirstOrDefault();
                    returnUrl = string.IsNullOrEmpty(returnUrl) ? "http://127.0.0.1:9301/external-callback" : returnUrl;
                    context.Response.Redirect($"{returnUrl}?error=access_denied");
                    context.HandleResponse();
                    return Task.CompletedTask;
                };
            });
        }

        var ms = configuration.GetSection("Authentication:Microsoft");
        if (!string.IsNullOrWhiteSpace(ms["ClientId"]))
        {
            authBuilder.AddMicrosoftAccount(op =>
            {
                op.ClientId = ms["ClientId"]!;
                op.ClientSecret = ms["ClientSecret"] ?? string.Empty;
                op.CallbackPath = "/auth/signin-microsoft";
                op.Events.OnRemoteFailure = context =>
                {
                    var returnUrl = context.Request.Query["returnUrl"].FirstOrDefault();
                    returnUrl = string.IsNullOrEmpty(returnUrl) ? "http://127.0.0.1:9301/external-callback" : returnUrl;
                    context.Response.Redirect($"{returnUrl}?error=access_denied");
                    context.HandleResponse();
                    return Task.CompletedTask;
                };
            });
        }

        return services;
    }

    private static void ConfigurePermissionCaching(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (!environment.IsEnvironment("Testing"))
        {
            var redisConnectionString = configuration["Redis:ConnectionString"] ?? configuration["ConnectionStrings:Redis"];
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
            }
        }

#pragma warning disable EXTEXP0018
        services.AddHybridCache(options => options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(10),
            LocalCacheExpiration = TimeSpan.FromMinutes(5)
        });
#pragma warning restore EXTEXP0018
    }
}
