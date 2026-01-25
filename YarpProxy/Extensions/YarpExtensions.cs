namespace YarpProxy.Extensions;

public static class YarpExtensions
{
    public static IServiceCollection AddYarpProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
        .LoadFromConfig(configuration.GetSection("ReverseProxy"))
        .AddTransforms(transformContext =>
        {
            transformContext.AddRequestHeader("X-Custom-Header", "YarpProxy");

            transformContext.AddPathRemovePrefix("/api/v1");
        });

        return services;
    }
}
