namespace Auth.TimeCafe.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IJwtService, JwtService>();

        var jwtSection = configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var signingKey = jwtSection["SigningKey"] ??
            throw new InvalidOperationException("Jwt:SigningKey missing");
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

#if (DEBUG)
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["Access-Token"];
                            return Task.CompletedTask;
                        }
                    };
                }
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
                op.CallbackPath = "/signin-google";
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
                op.CallbackPath = "/signin-microsoft";
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
}
