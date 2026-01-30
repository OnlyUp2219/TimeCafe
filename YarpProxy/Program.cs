var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSpaCorsConfiguration();

builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

builder.Services.AddYarpProxy(builder.Configuration);
builder.Services.AddScalarConfiguration();

builder.Services.AddAuthenticationConfiguration(builder.Configuration);

var app = builder.Build();

app.UseSpaCorsConfiguration();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.UseScalarConfiguration();

app.MapGet("/", () => "Hello World!");
app.MapGet("/health", () => Results.Ok("OK"));

app.Run();
