using Auth.TimeCafe.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDatabase(builder.Configuration);

// Identity
builder.Services.AddIdentityConfiguration(builder.Configuration);

// Authentication: JWT + external providers
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger param 
builder.Services.AddSwaggerConfiguration(builder.Configuration);

// CORS 
var corsPolicyName = builder.Configuration.GetSection("CORS");
builder.Services.AddCorsConfiguration(builder.Configuration, corsPolicyName["PolicyName"]);

// Carter
builder.Services.AddCarter();

// MassTransit
builder.Services.AddRabbitMqMessaging(builder.Configuration);

var app = builder.Build();

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
app.UseCors(corsPolicyName["PolicyName"]);

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

// Встроенные Identity API эндпоинты
app.MapIdentityApi<IdentityUser>();

// Внешние логины
app.MapControllers();

app.Run();


