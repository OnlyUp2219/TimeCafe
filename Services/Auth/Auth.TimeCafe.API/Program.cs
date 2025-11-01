using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDatabase(builder.Configuration);

// Identity
builder.Services.AddIdentityConfiguration(builder.Configuration);

// Authentication: JWT + external providers
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddAuthorization();

// Email sender
builder.Services.AddEmailSender(builder.Configuration);

// SMS services (Twilio + Rate Limiting)
builder.Services.AddSmsServices();

// CQRS (MediatR + Pipeline Behaviors)
builder.Services.AddUserProfileCqrs();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Swagger param 
builder.Services.AddSwaggerConfiguration(builder.Configuration);

// CORS 
var corsPolicyName = builder.Configuration.GetSection("CORS");
builder.Services.AddCorsConfiguration(corsPolicyName["PolicyName"]);

// Carter
builder.Services.AddCarter();

// MassTransit (отключён для разработки без RabbitMQ)
// builder.Services.AddRabbitMqMessaging(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var roleService = scope.ServiceProvider.GetRequiredService<IUserRoleService>();
    await roleService.EnsureRolesCreatedAsync();
    await SeedData.SeedAdminAsync(scope.ServiceProvider);
}


// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeCafe Auth API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors(corsPolicyName["PolicyName"] ?? "");

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

// Встроенные Identity API эндпоинты
app.MapIdentityApi<IdentityUser>();

// Внешние логины
app.MapControllers();

await app.RunAsync();
