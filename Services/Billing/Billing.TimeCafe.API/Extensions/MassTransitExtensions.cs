namespace Billing.TimeCafe.API.Extensions;



public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
        {
            services.AddMassTransit(x =>
            {
                x.AddBillingMassTransit();

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }

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

                cfg.Message<UserRegisteredEvent>(e => e.SetEntityName("user-registered"));

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
