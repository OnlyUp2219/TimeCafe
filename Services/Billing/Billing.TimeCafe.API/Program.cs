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

// Authentication & Authorization
builder.Services.AddJwtAuthenticationConfiguration(builder.Configuration);
builder.Services.AddAuthorization();

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

app.MapPost("/payments/webhook/stripe", async (
    [FromServices] ISender sender,
    [FromBody] StripeWebhookPayload payload,
    HttpRequest request) =>
{
    var signature = request.Headers["Stripe-Signature"].ToString();
    var command = new ProcessStripeWebhookCommand(payload, signature);
    var result = await sender.Send(command);
    return result.ToHttpResultV2(onSuccess: _ => Results.Ok());
})
.AllowAnonymous();

var billingGroup = app.MapGroup("/billing");
billingGroup.MapCarter();
billingGroup.MapControllers();

app.MapGet("/health", () => Results.Ok("OK"))
    .AllowAnonymous();

await app.RunAsync();

public partial class Program { }


