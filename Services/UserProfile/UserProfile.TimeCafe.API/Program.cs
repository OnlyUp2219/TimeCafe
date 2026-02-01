var builder = WebApplication.CreateBuilder(args);

var sharedSettingsPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "appsettings.shared.json"));
builder.Configuration.AddJsonFile(sharedSettingsPath, optional: true, reloadOnChange: true);

// Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

// CORS
var corsPolicyName = builder.Configuration["CORS:PolicyName"]
    ?? throw new InvalidOperationException("CORS:PolicyName is not configured.");
builder.Services.AddCorsConfiguration(corsPolicyName);

builder.Services.AddJwtAuthenticationConfiguration(builder.Configuration);

// MassTransit with RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration, builder.Environment);

// Redis
builder.Services.AddRedis(builder.Configuration);

// DbContext
builder.Services.AddUserProfileDatabase(builder.Configuration);

// Repositories
builder.Services.AddUserProfilePersistence();

// CQRS (MediatR + Pipeline Behaviors)
builder.Services.AddUserProfileCqrs();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddPermissionAuthorizationDev();
}
else
{
    // TODO : Register real IPermissionService here
}

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

app.UseAuthentication();
app.UseAuthorization();

// Swagger (Development only)
app.UseSwaggerDevelopment();

// Endpoints
var userProfileGroup = app.MapGroup("/userprofile");
userProfileGroup.MapCarter();

app.MapGet("/health", () => Results.Ok("OK"))
    .AllowAnonymous();


await app.RunAsync();

public partial class Program { }


