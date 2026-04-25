namespace Billing.TimeCafe.Application.DependencyInjection;

public static class CqrsDependencyInjection
{
    public static IServiceCollection AddBillingCqrs(this IServiceCollection services) =>
        services.AddCqrs(Assembly.GetExecutingAssembly());
}
