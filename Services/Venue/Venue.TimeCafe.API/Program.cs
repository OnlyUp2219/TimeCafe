using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.AddSharedConfiguration();

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

builder.Services.AddValidatedOptions<BillingApiOptions>(builder.Configuration, "Services:Billing");
builder.Services.Configure<Venue.TimeCafe.Application.Options.VenuePricingOptions>(builder.Configuration.GetSection("VenuePricing"));

// HttpClient
builder.Services.AddTransient<AuthorizationDelegatingHandler>();
builder.Services.AddHttpClient("BillingApi", (sp, client) =>
{
    var billingOptions = sp.GetRequiredService<IOptionsSnapshot<BillingApiOptions>>().Value;
    client.BaseAddress = new Uri(billingOptions.BaseUrl, UriKind.Absolute);
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



