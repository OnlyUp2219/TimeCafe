var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

// CORS
var corsPolicyName = builder.Configuration["CORS:PolicyName"]
    ?? throw new InvalidOperationException("CORS:PolicyName is not configured.");
builder.Services.AddCorsConfiguration(corsPolicyName);

// MassTransit with RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration);

// Redis
builder.Services.AddRedis(builder.Configuration);

// DbContext
builder.Services.AddUserProfileDatabase(builder.Configuration);

// Repositories
builder.Services.AddUserProfilePersistence();

// CQRS (MediatR + Pipeline Behaviors)
builder.Services.AddUserProfileCqrs();

// S3 storage (photos)
builder.Services.AddS3(builder.Configuration);

// Photo moderation
builder.Services.AddHttpClient<IPhotoModerationService, SightenginePhotoModerationService>();

// Swagger & Carter
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration(builder.Configuration);
builder.Services.AddCarter();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

await app.ApplyMigrationsAsync();

app.UseCors(corsPolicyName);

// Swagger (Development only)
app.UseSwaggerDevelopment();

// Endpoints
app.MapCarter();

await app.RunAsync();

public partial class Program { }


