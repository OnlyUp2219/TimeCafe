namespace Auth.TimeCafe.API.Extensions;

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
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host);
                cfg.Publish<UserRegisteredEvent>(p => p.ExchangeType = "fanout");
            });
        });

        return services;
    }
}