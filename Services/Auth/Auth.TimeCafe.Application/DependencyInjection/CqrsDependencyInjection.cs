namespace Auth.TimeCafe.Application.DependencyInjection;

public static class CqrsDependencyInjection
{
    public static IServiceCollection AddAuthCqrs(this IServiceCollection services) =>
        services.AddCqrs(Assembly.GetExecutingAssembly());
}
