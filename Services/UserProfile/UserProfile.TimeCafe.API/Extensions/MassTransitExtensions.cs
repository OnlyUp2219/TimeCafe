using BuildingBlocks.Events;

namespace UserProfile.TimeCafe.API.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var useRealInfrastructureInTests = configuration.GetValue<bool>("IntegrationTests:UseRealInfrastructure");

        if (environment.IsEnvironment("Testing") && !useRealInfrastructureInTests)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<UserRegisteredConsumer>();
                x.AddConsumer<UserProfile.TimeCafe.Infrastructure.Consumers.VisitCompletedEventConsumer>();

                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            });

            return services;
        }

        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.UsePostgres();

                o.UseBusOutbox();
            });

            x.AddConsumer<UserRegisteredConsumer>();
            x.AddConsumer<UserProfile.TimeCafe.Infrastructure.Consumers.VisitCompletedEventConsumer>();

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

                cfg.Message<UserRegisteredEvent>(e => e.SetEntityName("user-registered"));

                cfg.Message<VisitCompletedEvent>(e => e.SetEntityName("visit-completed"));
                cfg.Publish<VisitCompletedEvent>(p => p.ExchangeType = "fanout");

                cfg.ReceiveEndpoint("user-profile.user-registered", e =>
                {
                    e.Durable = true;
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureConsumer<UserRegisteredConsumer>(context);
                });

                cfg.ReceiveEndpoint("user-profile.visit-completed", e =>
                {
                    e.Durable = true;
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureConsumer<UserProfile.TimeCafe.Infrastructure.Consumers.VisitCompletedEventConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.Configure<MassTransitHostOptions>(options => options.StopTimeout = TimeSpan.FromSeconds(30));

        return services;
    }
}
