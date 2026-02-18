namespace Venue.TimeCafe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVenuePersistence(this IServiceCollection services)
    {
        services.AddScoped<ITariffRepository, TariffRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<IThemeRepository, ThemeRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();
        services.AddScoped<IVisitBalancePolicyService, VisitBalancePolicyService>();

        return services;
    }
}
