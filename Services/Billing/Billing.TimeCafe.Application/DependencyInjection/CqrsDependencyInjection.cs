namespace Billing.TimeCafe.Application.DependencyInjection;

public static class CqrsDependencyInjection
{
    public static IServiceCollection AddBillingCqrs(this IServiceCollection services)
    {
        services.AddScoped<IInvoicePaymentStrategy, OnlineInvoicePaymentStrategy>();
        services.AddScoped<IInvoicePaymentStrategy, CashInvoicePaymentStrategy>();
        services.AddScoped<IInvoicePaymentStrategy, CardInvoicePaymentStrategy>();

        return services.AddCqrs(Assembly.GetExecutingAssembly());
    }
}
