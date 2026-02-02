namespace YarpProxy.Extensions;

public static class CqrsDependencyInjection
{
    public static IServiceCollection AddYarpCqrs(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(assembly);

        services.AddValidatorsFromAssembly(assembly);

        // Pipeline behaviors (порядок регистрации имеет значение: Validation -> Logging -> Performance -> ErrorHandling)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));

        return services;
    }
}