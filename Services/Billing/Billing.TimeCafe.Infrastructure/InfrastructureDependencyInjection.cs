namespace Billing.TimeCafe.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddBillingInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IBalanceRepository, BalanceRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }

    public static void AddBillingMassTransit(this IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<VisitCompletedEventConsumer>();
        cfg.AddConsumer<UserRegisteredEventConsumer>();
    }
}
