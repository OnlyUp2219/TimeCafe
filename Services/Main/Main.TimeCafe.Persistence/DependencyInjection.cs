
using Microsoft.Extensions.DependencyInjection;
using TimeCafe.Core.Contracts.Repositories;
using TimeCafe.Persistence.Repositories;

namespace TimeCafe.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IFinancialRepository, FinancialRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IClientAdditionalInfoRepository, ClientAdditionalInfoRepository>();
        services.AddScoped<IBillingTypeRepository, BillingTypeRepository>();
        services.AddScoped<ITariffRepository, TariffRepository>();
        services.AddScoped<IThemeRepository, ThemeRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();

        return services;
    }
}
