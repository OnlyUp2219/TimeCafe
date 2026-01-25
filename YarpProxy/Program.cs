var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilogConfiguration(builder.Configuration);
builder.Host.UseSerilog();

builder.Services.AddYarpProxy(builder.Configuration);
builder.Services.AddScalarConfiguration();


var app = builder.Build();

app.MapReverseProxy();

app.UseScalarConfiguration();

app.MapGet("/", () => "Hello World!");

app.Run();
