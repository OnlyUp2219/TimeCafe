var builder = WebApplication.CreateBuilder(args);

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

// MassTransit (отключён для разработки без RabbitMQ)
// builder.Services.AddRabbitMqMessaging(builder.Configuration);

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

//app.MapIdentityApi<IdentityUser>();

app.MapControllers();

await app.RunAsync();

public partial class Program { }
