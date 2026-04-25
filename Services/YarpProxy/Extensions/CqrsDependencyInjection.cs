namespace YarpProxy.Extensions;

public static class CqrsDependencyInjection
{
    public static IServiceCollection AddYarpCqrs(this IServiceCollection services) =>
        services.AddCqrs(Assembly.GetExecutingAssembly());
}
