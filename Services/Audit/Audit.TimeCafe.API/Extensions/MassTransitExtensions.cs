using Audit.TimeCafe.Infrastructure.Consumers;
using MassTransit;

namespace Audit.TimeCafe.API.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<SaveAuditConsumer>();
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            });
            return services;
        }

        services.AddMassTransit(x =>
        {
            x.AddConsumer<SaveAuditConsumer>();
            x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

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
                    var port = configuration.GetValue<ushort>("RabbitMQ:Port", 5672);
                    var rabbitMqHost = host.Replace("tcp://", "rabbitmq://");

                    cfg.Host(rabbitMqHost, port, "/", h =>
                    {
                        h.Username(rabbitMqSection["Username"] ?? "guest");
                        h.Password(rabbitMqSection["Password"] ?? "guest");
                    });
                }

                cfg.ConfigureEndpoints(context);
            });
        });

        services.Configure<MassTransitHostOptions>(options => options.StopTimeout = TimeSpan.FromSeconds(30));

        return services;
    }
}
