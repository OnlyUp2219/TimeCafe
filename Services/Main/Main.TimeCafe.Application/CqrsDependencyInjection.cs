using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace TimeCafe.Application;

public static class CqrsDependencyInjection
{
    public static IServiceCollection AddCqrs(this IServiceCollection services)
    {
        var cqrsAssembly = Assembly.GetExecutingAssembly();
        var handlerAssemblies = new[] { cqrsAssembly };

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(handlerAssemblies));
        return services;
    }
}
