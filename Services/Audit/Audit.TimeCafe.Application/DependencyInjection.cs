namespace Audit.TimeCafe.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditCqrs(this IServiceCollection services) =>
        services.AddCqrs(Assembly.GetExecutingAssembly());
}
