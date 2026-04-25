using BuildingBlocks.Events;

namespace Venue.TimeCafe.API.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.UsePostgres();

                o.UseBusOutbox();
            });

            x.AddConsumer<Venue.TimeCafe.Infrastructure.Consumers.UserDiscountUpdatedEventConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = configuration["RabbitMQ:ConnectionString"] ?? configuration.GetConnectionString("rabbitmq");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    cfg.Host(connectionString);
                }
                else
                {
                    var rabbitMqSection = configuration.GetSection("RabbitMQ");
                    var host = rabbitMqSection["Host"] ?? "localhost";
                    var virtualHost = rabbitMqSection["VirtualHost"] ?? "/";
                    var port = configuration.GetValue<ushort>("RabbitMQ:Port", 5672);
                    var rabbitMqHost = host.Replace("tcp://", "rabbitmq://");

                    cfg.Host(rabbitMqHost, port, virtualHost, h =>
                    {
                        h.Username(rabbitMqSection["Username"] ?? "guest");
                        h.Password(rabbitMqSection["Password"] ?? "guest");
                    });
                }

                cfg.Message<VisitCompletedEvent>(e => e.SetEntityName("visit-completed"));
                cfg.Publish<VisitCompletedEvent>(p => p.ExchangeType = "fanout");

                cfg.ReceiveEndpoint("venue-user-discount-updated", e =>
                {
                    e.ConfigureConsumer<Venue.TimeCafe.Infrastructure.Consumers.UserDiscountUpdatedEventConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.Configure<MassTransitHostOptions>(options => options.StopTimeout = TimeSpan.FromSeconds(30));

        return services;
    }
}

