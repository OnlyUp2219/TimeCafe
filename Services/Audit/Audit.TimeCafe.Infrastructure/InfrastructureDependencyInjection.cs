using Microsoft.Extensions.DependencyInjection;

namespace Audit.TimeCafe.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddAuditInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
