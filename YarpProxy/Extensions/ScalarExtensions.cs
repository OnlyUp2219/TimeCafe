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
        app.MapScalarApiReference(options =>
        {
            var devBearerToken = app.Configuration["Scalar:DevBearerToken"];
            var autoGenerateAdminToken = app.Configuration.GetValue("Scalar:AutoGenerateAdminToken", app.Environment.IsDevelopment());

            if (string.IsNullOrWhiteSpace(devBearerToken)
                && autoGenerateAdminToken
                && app.Environment.IsDevelopment())
            {
                devBearerToken = GenerateAdminJwt(app.Configuration);
            }

            options.WithTitle("TimeCafe Gateway")
                   .WithTheme(ScalarTheme.DeepSpace)
                   .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                   .WithOpenApiRoutePattern("/openapi/{documentName}.json")
                   .AddPreferredSecuritySchemes("Bearer")
                   .AddHttpAuthentication("Bearer", auth =>
                   {
                       if (!string.IsNullOrWhiteSpace(devBearerToken))
                       {
                           auth.Token = devBearerToken;
                       }
                   });

            options.Authentication = new ScalarAuthenticationOptions
            {
                PreferredSecuritySchemes = ["Bearer"]
            };

            options.AddDocument(
           documentName: "auth",
           title: "TimeCafe Auth API",
           routePattern: "/openapi/auth.json",
           isDefault: true);

            options.AddDocument(
                documentName: "userprofile",
                title: "TimeCafe UserProfile API",
                routePattern: "/openapi/userprofile.json",
                isDefault: false);

            options.AddDocument(
                documentName: "venue",
                title: "TimeCafe Venue API",
                routePattern: "/openapi/venue.json",
                isDefault: false);

            options.AddDocument(
                documentName: "billing",
                title: "TimeCafe Billing API",
                routePattern: "/openapi/billing.json",
                isDefault: false);
        });

        return app;
    }

    private static string GenerateAdminJwt(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
        var signingKey = jwtSection["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var userId = "00000000-0000-0000-0000-000000000001";
        var now = DateTime.UtcNow;
        var expires = now.AddHours(12);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("sub", userId),
            new(ClaimTypes.Role, "admin")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
