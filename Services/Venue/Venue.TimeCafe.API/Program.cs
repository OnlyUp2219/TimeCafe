var builder = WebApplication.CreateBuilder(args);

var sharedSettingsPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "appsettings.shared.json"));
builder.Configuration.AddJsonFile(sharedSettingsPath, optional: true, reloadOnChange: true);

// Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

// CORS
var corsPolicyName = builder.Services.AddCorsConfiguration(builder.Configuration);

builder.Services.AddJwtAuthenticationConfiguration(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// MassTransit with RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration);

// Redis
builder.Services.AddRedis(builder.Configuration);

// HttpClient
builder.Services.AddTransient<AuthorizationDelegatingHandler>();
builder.Services.AddHttpClient("BillingApi", (_, client) =>
{
    var billingBaseUrl = builder.Configuration["Services:Billing:BaseUrl"] ?? "http://localhost:8004";
    client.BaseAddress = new Uri(billingBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddHttpMessageHandler<AuthorizationDelegatingHandler>();

// DbContext
builder.Services.AddPostgresDatabase<ApplicationDbContext>(builder.Configuration);

// AutoMapper
builder.Services.AddVenueAutoMapper();

// Repositories
builder.Services.AddVenuePersistence();

// CQRS (MediatR + Pipeline Behaviors)
builder.Services.AddVenueCqrs();

// Swagger & Carter
builder.Services.AddControllers();
builder.Services.AddOpenApiConfiguration("TimeCafe Venue API");
builder.Services.AddCarter();

// HealthChecks
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

await app.ApplyMigrationsAsync<ApplicationDbContext>();
await app.SeedFrontendDataAsync();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseOpenApiDevelopment("TimeCafe Venue API");

var venueGroup = app.MapGroup("/venue");
venueGroup.MapCarter();
venueGroup.MapControllers();

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.UseHealthChecks();

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}


