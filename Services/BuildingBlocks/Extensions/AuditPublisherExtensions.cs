using BuildingBlocks.Providers;

namespace BuildingBlocks.Extensions;

public static class AuditPublisherExtensions
{
    public static IServiceCollection AddAuditConsumer(this IServiceCollection services)
    {
        services.AddSingleton<MassTransitAuditDataProvider>();
        services.AddSingleton<AuditDataProvider>(sp => sp.GetRequiredService<MassTransitAuditDataProvider>());
        return services;
    }

    public static void ConfigureAuditProvider(this WebApplication app)
    {
        var provider = app.Services.GetRequiredService<MassTransitAuditDataProvider>();
        Audit.Core.Configuration.Setup().UseCustomProvider(provider);
    }
}
