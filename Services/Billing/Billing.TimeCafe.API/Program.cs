var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

// CORS
var corsPolicyName = builder.Configuration["CORS:PolicyName"]
    ?? throw new InvalidOperationException("CORS:PolicyName is not configured.");
builder.Services.AddCorsConfiguration(corsPolicyName);

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
builder.Services.AddAuthorization();

// Swagger & Carter
builder.Services.AddControllers();
builder.Services.AddOpenApiConfiguration("TimeCafe Billing API");
builder.Services.AddCarter();

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

app.MapGet("/health", () => Results.Ok("OK"))
    .AllowAnonymous();

await app.RunAsync();

public partial class Program;


