using Venue.TimeCafe.Application.DependencyInjection;

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
builder.Services.AddVenueDatabase(builder.Configuration);

// AutoMapper
builder.Services.AddVenueAutoMapper();

// Repositories
builder.Services.AddVenuePersistence();

// CQRS (MediatR + Pipeline Behaviors)
builder.Services.AddVenueCqrs();

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

app.MapGet("/health", () => Results.Ok("OK"))
    .AllowAnonymous();

await app.RunAsync();

public partial class Program { }


