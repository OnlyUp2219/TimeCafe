namespace Billing.TimeCafe.API.Extensions;

using Billing.TimeCafe.Infrastructure;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqSection = configuration.GetSection("RabbitMQ");
        if (!rabbitMqSection.Exists())
            throw new InvalidOperationException("RabbitMQ configuration section is missing.");

        var host = rabbitMqSection["Host"] ?? throw new InvalidOperationException("RabbitMQ:Host is not configured.");

        services.AddMassTransit(x =>
        {
            x.AddBillingMassTransit();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqSection["Host"]!, h =>
                {
                    h.Username(rabbitMqSection["Username"]!);
                    h.Password(rabbitMqSection["Password"]!);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.Configure<MassTransitHostOptions>(options =>
        {
            options.StopTimeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
