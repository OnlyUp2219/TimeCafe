using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformContext =>
    {
        transformContext.AddRequestHeader("X-Custom-Header", "YarpProxy");

        transformContext.AddPathPrefix("/api/v1/");
    });

var app = builder.Build();

app.MapReverseProxy();

app.MapGet("/", () => "Hello World!");

app.Run();
