var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddSharedConfiguration();

// Serilog
builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

// DbContext
builder.Services.AddPostgresDatabase<ApplicationDbContext>(builder.Configuration);

// Infrastructure
builder.Services.AddAuthInfrastructure();

// Identity
builder.Services.AddIdentityConfiguration();

// Authentication: JWT + external providers
builder.Services.AddAuthenticationConfiguration(builder.Configuration, builder.Environment);

// Email sender
builder.Services.AddEmailSender(builder.Configuration);

// SMS services (Twilio + Rate Limiting)
builder.Services.AddSmsServices(builder.Configuration);

// CQRS (MediatR + Pipeline Behaviors)
builder.Services.AddAuthCqrs();

builder.Services.AddControllers();

builder.Services.AddOpenApiConfiguration("TimeCafe Auth API");
builder.Services.AddGrpc();

// Rate Limiter
builder.Services.AddCustomRateLimiter(builder.Configuration);

// HealthChecks
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

// CORS
var corsPolicyName = builder.Services.AddCorsConfiguration(builder.Configuration);

// Carter
builder.Services.AddCarter();

// MassTransit with RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration, builder.Environment);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto |
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseForwardedHeaders();

await app.ApplyMigrationsAsync();
await app.SeedRolesAndPermissions();

using (var warmupScope = app.Services.CreateScope())
{
    var warmupContext = warmupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var warmupUserRepository = warmupScope.ServiceProvider.GetRequiredService<IUserRepository>();

    await warmupContext.UserRoles
        .Join(warmupContext.Set<IdentityRoleClaim<Guid>>().Where(rc => rc.ClaimType == CustomClaimTypes.Permissions),
            ur => ur.RoleId, rc => rc.RoleId, (_, rc) => rc.ClaimValue)
        .Take(1)
        .ToListAsync();

    var (warmupUsersPage1, _) = await warmupUserRepository.GetUsersPageAsync(1, 1, null, null, CancellationToken.None);
    if (warmupUsersPage1.Count > 0)
    {
        await warmupUserRepository.GetUsersRolesBatchAsync(warmupUsersPage1.Select(u => u.Id), CancellationToken.None);
    }

    var (warmupUsersPage20, _) = await warmupUserRepository.GetUsersPageAsync(1, 20, null, null, CancellationToken.None);
    if (warmupUsersPage20.Count > 0)
    {
        await warmupUserRepository.GetUsersRolesBatchAsync(warmupUsersPage20.Select(u => u.Id), CancellationToken.None);
    }
}

if (app.Environment.IsDevelopment())
{
    await app.SeedDevelopmentUsersAsync();
}

app.UseOpenApiDevelopment("TimeCafe Auth API");

app.UseCors(corsPolicyName);

app.UseMiddleware<RateLimitCounterMiddleware>();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

var authGroup = app.MapGroup("/auth");
authGroup.MapCarter();
authGroup.MapControllers();

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
app.MapGrpcService<Auth.TimeCafe.API.Services.PermissionGrpcService>();

app.UseHealthChecks();
app.MapDefaultEndpoints();

authGroup.MapGet("/test-publish", async (IPublishEndpoint pub) =>
{
    await pub.Publish(new UserRegisteredEvent
    {
        UserId = Guid.NewGuid(),
        Email = "test@example.com"
    });

    return Results.Ok("Событие отправлено!");
});

authGroup.MapGet("/test-yarp", () =>
{
    var user = new ApplicationUser()
    {
        Id = Guid.NewGuid(),
        Email = $"klimenkokov{Guid.NewGuid()}@gmail.com"
    };
    return Results.Ok(user);
});

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}

