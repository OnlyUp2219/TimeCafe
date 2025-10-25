namespace UserProfile.TimeCafe.API.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserRegisteredConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetValue<string>("RabbitMq:Host") ?? "rabbitmq://localhost");

                cfg.ReceiveEndpoint("user-register-queue", e =>
                {
                    e.ConfigureConsumer<UserRegisteredConsumer>(context);
                });
            });
        });

        return services;
    }
}
