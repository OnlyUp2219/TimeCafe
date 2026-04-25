namespace Venue.TimeCafe.Application.DependencyInjection;

public static class CqrsDependencyInjection
{
    public static IServiceCollection AddVenueCqrs(this IServiceCollection services) =>
        services.AddCqrs(Assembly.GetExecutingAssembly());
}

