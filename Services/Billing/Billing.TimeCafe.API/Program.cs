using Billing.TimeCafe.Application.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

// CORS
var corsPolicyName = builder.Configuration["CORS:PolicyName"]
    ?? throw new InvalidOperationException("CORS:PolicyName is not configured.");
builder.Services.AddCorsConfiguration(corsPolicyName);

// MassTransit with RabbitMQ (commented out until consumers are ready)
// builder.Services.AddRabbitMqMessaging(builder.Configuration);

// Redis
builder.Services.AddRedis(builder.Configuration);

// DbContext
builder.Services.AddBillingDatabase(builder.Configuration);

// Infrastructure (repositories)
builder.Services.AddBillingInfrastructure();

// CQRS (MediatR + Pipeline Behaviors)
builder.Services.AddBillingCqrs();

// Swagger & Carter
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiConfiguration();
builder.Services.AddCarter();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

await app.ApplyMigrationsAsync();

app.UseCors(corsPolicyName);

app.UseSwaggerDevelopment();

app.MapCarter();
app.MapControllers();

await app.RunAsync();

public partial class Program { }


