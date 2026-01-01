using Billing.TimeCafe.Domain.Repositories;
using Billing.TimeCafe.Infrastructure.Repositories;

namespace Billing.TimeCafe.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddBillingInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IBalanceRepository, BalanceRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }
}
