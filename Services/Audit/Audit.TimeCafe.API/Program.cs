var builder = WebApplication.CreateBuilder(args);
builder.AddSharedConfiguration();

// Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

// Redis
builder.Services.AddRedis(builder.Configuration);

// DbContext
builder.Services.AddAuditPostgresDatabase(builder.Configuration);

// Infrastructure
builder.Services.AddAuditInfrastructure();
builder.Services.AddAuditCqrs();
var corsPolicyName = builder.Services.AddCorsConfiguration(builder.Configuration);

// Swagger & Carter
builder.Services.AddControllers();
builder.Services.AddOpenApiConfiguration("TimeCafe Audit API");
builder.Services.AddCarter();

// MassTransit with RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration, builder.Environment);

// HealthChecks
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

await app.ApplyMigrationsAsync<ApplicationDbContext>();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseOpenApiDevelopment("TimeCafe Audit API");

var auditGroup = app.MapGroup("/audit");
auditGroup.MapCarter();
auditGroup.MapControllers();

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.UseHealthChecks();

await app.RunAsync();

public partial class Program;
