var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddSharedConfiguration();

// Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

// CORS
var corsPolicyName = builder.Services.AddCorsConfiguration(builder.Configuration);

// MassTransit with RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration, builder.Environment);
builder.Services.AddAuditConsumer();

// Redis
builder.Services.AddRedis(builder.Configuration);

// DbContext and Auditing
builder.Services.AddAuditDatabase<ApplicationDbContext>();
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

builder.Services.AddHostedService<Billing.TimeCafe.API.Services.StripeCliRunner>();

var app = builder.Build();

app.ConfigureAuditProvider();

app.UseMiddleware<ExceptionHandlingMiddleware>();

await app.ApplyMigrationsAsync<ApplicationDbContext>();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseOpenApiDevelopment("TimeCafe Billing API");

var billingGroup = app.MapGroup("/billing");
billingGroup.MapCarter();
billingGroup.MapControllers();

app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

app.UsePrometheusMetrics();
app.UseHealthChecks();
app.MapDefaultEndpoints();

await app.RunAsync();

public partial class Program;


