using BuildingBlocks.Events;

namespace Venue.TimeCafe.API.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqSection = configuration.GetSection("RabbitMQ");
        if (!rabbitMqSection.Exists())
            throw new InvalidOperationException("RabbitMQ configuration section is missing.");

        var host = rabbitMqSection["Host"] ?? throw new InvalidOperationException("RabbitMQ:Host is not configured.");
        var virtualHost = rabbitMqSection["VirtualHost"] ?? "/";
        var hasPort = ushort.TryParse(rabbitMqSection["Port"], out var port);

        services.AddMassTransit(x => x.UsingRabbitMq((context, cfg) =>
            {
                if (hasPort)
                {
                    cfg.Host(host, port, virtualHost, h =>
                    {
                        h.Username(rabbitMqSection["Username"]!);
                        h.Password(rabbitMqSection["Password"]!);
                    });
                }
                else
                {
                    cfg.Host(host, h =>
                    {
                        h.Username(rabbitMqSection["Username"]!);
                        h.Password(rabbitMqSection["Password"]!);
                    });
                }

                cfg.Message<VisitCompletedEvent>(e => e.SetEntityName("visit-completed"));
                cfg.Publish<VisitCompletedEvent>(p => p.ExchangeType = "fanout");

                cfg.ConfigureEndpoints(context);
            }));

        services.Configure<MassTransitHostOptions>(options => options.StopTimeout = TimeSpan.FromSeconds(30));

        return services;
    }
}
