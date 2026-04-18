using BuildingBlocks.Authentication;

namespace Auth.TimeCafe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRbacRepository, RbacRepository>();
        return services;
    }
}