var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

// DbContext
builder.Services.AddDatabase(builder.Configuration);

// Identity
builder.Services.AddIdentityConfiguration(builder.Configuration);

// Authentication: JWT + external providers
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddAuthorization();

// Permission system registration
builder.Services.AddPermissionAuthorization();

// Email sender
builder.Services.AddEmailSender(builder.Configuration);

// SMS services (Twilio + Rate Limiting)
builder.Services.AddSmsServices(builder.Configuration);

// CQRS (MediatR + Pipeline Behaviors)
builder.Services.AddUserProfileCqrs();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger param 
builder.Services.AddSwaggerConfiguration(builder.Configuration);

// Rate Limiter
builder.Services.AddCustomRateLimiter(builder.Configuration);

// CORS
var corsPolicyName = builder.Configuration["CORS:PolicyName"]
    ?? throw new InvalidOperationException("CORS:PolicyName is not configured.");
builder.Services.AddCorsConfiguration(corsPolicyName);

// Carter
builder.Services.AddCarter();

// MassTransit with RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

await app.ApplyMigrationsAsync();

app.UseSwaggerDevelopment();

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);

app.UseMiddleware<RateLimitCounterMiddleware>();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

app.MapControllers();

app.MapGet("/health", () => Results.Ok("OK"))
    .AllowAnonymous();

app.MapGet("/test-publish", async (IPublishEndpoint pub) =>
{
    await pub.Publish(new UserRegisteredEvent
    {
        UserId = Guid.NewGuid(),
        Email = "test@example.com"
    });

    return Results.Ok("Событие отправлено!");
});

app.MapGet("/test-yarp", async (IPublishEndpoint pub) =>
{
    var user = new ApplicationUser()
    {
        Id = Guid.NewGuid(),
        Email = $"klimenkokov{Guid.NewGuid()}@gmail.com"
    };
    return Results.Ok(user);
});

await app.RunAsync();

public partial class Program { }

