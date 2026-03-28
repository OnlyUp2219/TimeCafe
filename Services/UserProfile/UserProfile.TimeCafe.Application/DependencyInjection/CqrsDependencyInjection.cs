using System.Reflection;

namespace UserProfile.TimeCafe.Application.DependencyInjection;

public static class CqrsDependencyInjection
{
    public static IServiceCollection AddUserProfileCqrs(this IServiceCollection services) =>
        services.AddCqrs(Assembly.GetExecutingAssembly());
}
