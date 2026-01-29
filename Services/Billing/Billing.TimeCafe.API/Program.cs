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
builder.Services.AddBillingDatabase(builder.Configuration);

// Infrastructure (repositories)
builder.Services.AddBillingInfrastructure(builder.Configuration);

// CQRS (MediatR + Pipeline Behaviors)
builder.Services.AddBillingCqrs();

// Authentication & Authorization (for development - allows all)
builder.Services
    .AddAuthentication("DummyScheme")
    .AddScheme<AuthenticationSchemeOptions, DummyAuthenticationHandler>("DummyScheme", _ => { });
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Swagger & Carter
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiConfiguration();
builder.Services.AddCarter();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

await app.ApplyMigrationsAsync();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerDevelopment();

app.MapCarter();
app.MapControllers();

app.MapGet("/health", () => Results.Ok("OK"))
    .AllowAnonymous();

await app.RunAsync();

public partial class Program { }


