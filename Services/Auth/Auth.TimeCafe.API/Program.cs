var builder = WebApplication.CreateBuilder(args);

var sharedSettingsCandidates = new[]
{
    Path.Combine(builder.Environment.ContentRootPath, "appsettings.shared.json"),
    Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "appsettings.shared.json"))
};

var sharedSettingsPath = sharedSettingsCandidates.FirstOrDefault(File.Exists);
if (sharedSettingsPath is not null)
    builder.Configuration.AddJsonFile(sharedSettingsPath, optional: false, reloadOnChange: true);

// Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

// DbContext
builder.Services.AddPostgresDatabase<ApplicationDbContext>(builder.Configuration);

// Infrastructure
builder.Services.AddAuthInfrastructure();

// Identity
builder.Services.AddIdentityConfiguration();

// Authentication: JWT + external providers
builder.Services.AddAuthenticationConfiguration(builder.Configuration, builder.Environment);

// Email sender
builder.Services.AddEmailSender(builder.Configuration);

// SMS services (Twilio + Rate Limiting)
builder.Services.AddSmsServices(builder.Configuration);

// CQRS (MediatR + Pipeline Behaviors)
builder.Services.AddAuthCqrs();

builder.Services.AddControllers();

builder.Services.AddOpenApiConfiguration("TimeCafe Auth API");

// Rate Limiter
builder.Services.AddCustomRateLimiter(builder.Configuration);

// HealthChecks
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

// CORS
var corsPolicyName = builder.Services.AddCorsConfiguration(builder.Configuration);

// Carter
builder.Services.AddCarter();

// MassTransit with RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration, builder.Environment);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto |
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseForwardedHeaders();

await app.ApplyMigrationsAsync();
await app.SeedRolesAndPermissions();

if (app.Environment.IsDevelopment())
{
    await app.SeedDevelopmentUsersAsync();
}

app.UseOpenApiDevelopment("TimeCafe Auth API");

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);

app.UseMiddleware<RateLimitCounterMiddleware>();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

var authGroup = app.MapGroup("/auth");
authGroup.MapCarter();
authGroup.MapControllers();

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.UseHealthChecks();

authGroup.MapGet("/test-publish", async (IPublishEndpoint pub) =>
{
    await pub.Publish(new UserRegisteredEvent
    {
        UserId = Guid.NewGuid(),
        Email = "test@example.com"
    });

    return Results.Ok("Событие отправлено!");
});

authGroup.MapGet("/test-yarp", () =>
{
    var user = new ApplicationUser()
    {
        Id = Guid.NewGuid(),
        Email = $"klimenkokov{Guid.NewGuid()}@gmail.com"
    };
    return Results.Ok(user);
});

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}

