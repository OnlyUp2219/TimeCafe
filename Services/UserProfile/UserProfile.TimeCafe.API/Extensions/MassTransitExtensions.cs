using BuildingBlocks.Events;

namespace UserProfile.TimeCafe.API.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<UserRegisteredConsumer>();

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
            x.AddConsumer<UserRegisteredConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqSection["Host"]!, h =>
                {
                    h.Username(rabbitMqSection["Username"]!);
                    h.Password(rabbitMqSection["Password"]!);
                });

                cfg.Message<UserRegisteredEvent>(e => e.SetEntityName("user-registered"));

                cfg.Message<VisitCompletedEvent>(e => e.SetEntityName("visit-completed"));
                cfg.Publish<VisitCompletedEvent>(p =>
                {
                    p.ExchangeType = "fanout";
                });

                cfg.ReceiveEndpoint("user-profile.user-registered", e =>
                {
                    e.Durable = true;
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureConsumer<UserRegisteredConsumer>(context);
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
