var builder = WebApplication.CreateBuilder(args);

// CORS
var corsPolicyName = builder.Configuration["CORS:PolicyName"]
    ?? throw new InvalidOperationException("CORS:PolicyName is not configured.");
builder.Services.AddCorsConfiguration(corsPolicyName);

// MassTransit (отключён для разработки без RabbitMQ)
//builder.Services.AddRabbitMqMessaging(builder.Configuration);

// Redis
builder.Services.AddRedis(builder.Configuration);

// DbContext
builder.Services.AddVenueDatabase(builder.Configuration);

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

await app.RunAsync();

public partial class Program { }


