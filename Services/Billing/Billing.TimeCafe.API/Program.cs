var builder = WebApplication.CreateBuilder(args);
builder.AddSharedConfiguration();

// Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

// CORS
var corsPolicyName = builder.Services.AddCorsConfiguration(builder.Configuration);

// MassTransit with RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration, builder.Environment);

// Redis
builder.Services.AddRedis(builder.Configuration);

// DbContext
builder.Services.AddPostgresDatabase<ApplicationDbContext>(builder.Configuration);

// Infrastructure (repositories)
builder.Services.AddBillingInfrastructure(builder.Configuration);

// CQRS (MediatR + Pipeline Behaviors)
builder.Services.AddBillingCqrs();

// Authentication & Authorization
builder.Services.AddJwtAuthenticationConfiguration(builder.Configuration);

// Swagger & Carter
builder.Services.AddControllers();
builder.Services.AddOpenApiConfiguration("TimeCafe Billing API");
builder.Services.AddCarter();

// HealthChecks
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

await app.ApplyMigrationsAsync<ApplicationDbContext>();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseOpenApiDevelopment("TimeCafe Billing API");

var billingGroup = app.MapGroup("/billing");
billingGroup.MapCarter();
billingGroup.MapControllers();

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.UseHealthChecks();

await app.RunAsync();

public partial class Program;


