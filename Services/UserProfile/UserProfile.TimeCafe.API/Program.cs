using UserProfile.TimeCafe.API.Json;

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

// CORS
var corsPolicyName = builder.Services.AddCorsConfiguration(builder.Configuration);

builder.Services.AddJwtAuthenticationConfiguration(builder.Configuration);

// MassTransit with RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration, builder.Environment);

// Redis
builder.Services.AddRedis(builder.Configuration);

// DbContext
builder.Services.AddPostgresDatabase<ApplicationDbContext>(builder.Configuration);

// Repositories
builder.Services.AddUserProfilePersistence();

// CQRS (MediatR + Pipeline Behaviors)
builder.Services.AddUserProfileCqrs();

// S3 storage (photos)
builder.Services.AddS3(builder.Configuration);

// Photo moderation
builder.Services.AddHttpClient<IPhotoModerationService, SightenginePhotoModerationService>();

// Swagger & Carter
builder.Services.AddOpenApiConfiguration("TimeCafe UserProfile API");
builder.Services.AddCarter();

// HealthChecks
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new FlexibleDateOnlyJsonConverter()));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

await app.ApplyMigrationsAsync<ApplicationDbContext>();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseOpenApiDevelopment("TimeCafe UserProfile API");

// Endpoints
var userProfileGroup = app.MapGroup("/userprofile");
userProfileGroup.MapCarter();

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.UseHealthChecks();


await app.RunAsync();

public partial class Program
{
    protected Program() { }
}


