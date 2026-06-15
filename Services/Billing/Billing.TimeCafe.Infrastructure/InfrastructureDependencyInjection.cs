namespace Billing.TimeCafe.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddBillingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IBalanceRepository, BalanceRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IBillingTransactionExecutor, BillingTransactionExecutor>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<Billing.TimeCafe.Application.Options.FiscalOptions>(
            configuration.GetSection(Billing.TimeCafe.Application.Options.FiscalOptions.SectionName));
        services.AddScoped<Billing.TimeCafe.Application.Services.IFiscalService, Billing.TimeCafe.Infrastructure.Services.MockFiscalService>();

        services.AddValidatedOptions<StripeOptions>(
            configuration,
            "Stripe",
            options =>
            {
                if (string.IsNullOrWhiteSpace(options.SecretKey))
                    return false;
                if (string.IsNullOrWhiteSpace(options.PublishableKey))
                    return false;
                if (string.IsNullOrWhiteSpace(options.DefaultCurrency))
                    return false;
                return true;
            },
            "Критичные поля Stripe не заполнены: SecretKey, PublishableKey или DefaultCurrency отсутствуют в appsettings.json");

        services.AddScoped<IStripePaymentClient, StripePaymentClient>();

        return services;
    }

    public static void AddBillingMassTransit(this IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<VisitTimerStoppedEventConsumer>();
        cfg.AddConsumer<UserRegisteredEventConsumer>();
        cfg.AddConsumer<VisitApprovedEventConsumer>();
        cfg.AddConsumer<VisitRejectedEventConsumer>();
    }
}
