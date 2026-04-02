var builder = WebApplication.CreateBuilder(args);

var sharedSettingsPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "appsettings.shared.json"));
builder.Configuration.AddJsonFile(sharedSettingsPath, optional: true, reloadOnChange: true);

// Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

// DbContext
builder.Services.AddPostgresDatabase<ApplicationDbContext>(builder.Configuration);

// Identity
builder.Services.AddIdentityConfiguration();

// Authentication: JWT + external providers
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("DefaultUser", policy => policy.RequireAuthenticatedUser())
    .AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));

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

builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto |
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseForwardedHeaders();

await app.ApplyMigrationsAsync();

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

