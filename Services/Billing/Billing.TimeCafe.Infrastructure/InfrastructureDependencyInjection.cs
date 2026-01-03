namespace Billing.TimeCafe.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddBillingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IBalanceRepository, BalanceRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.Configure<StripeOptions>(configuration.GetSection("Stripe"));
        services.AddScoped<IStripePaymentClient, StripePaymentClient>();

        return services;
    }

    public static void AddBillingMassTransit(this IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<VisitCompletedEventConsumer>();
        cfg.AddConsumer<UserRegisteredEventConsumer>();
    }
}
