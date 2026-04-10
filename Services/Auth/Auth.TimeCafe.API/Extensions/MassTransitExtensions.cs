namespace Auth.TimeCafe.API.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var hasLicense =
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MT_LICENSE")) ||
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MT_LICENSE_PATH"));

        if (!hasLicense || environment.IsEnvironment("Testing"))
        {
            services.AddMassTransit(x => x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context)));

            return services;
        }

        var rabbitMqSection = configuration.GetSection("RabbitMQ");
        if (!rabbitMqSection.Exists())
            throw new InvalidOperationException("RabbitMQ configuration section is missing.");
        _ = rabbitMqSection["Host"] ?? throw new InvalidOperationException("RabbitMQ:Host is not configured.");

        services.AddMassTransit(x =>
        { 
            x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.UsePostgres(); 

                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqSection["Host"]!, h =>
                    {
                        h.Username(rabbitMqSection["Username"]!);
                        h.Password(rabbitMqSection["Password"]!);
                    });

                    cfg.Message<UserRegisteredEvent>(e => e.SetEntityName("user-registered"));
                    cfg.Publish<UserRegisteredEvent>(p => p.ExchangeType = "fanout");

                    cfg.Message<VisitCompletedEvent>(e => e.SetEntityName("visit-completed"));
                    cfg.Publish<VisitCompletedEvent>(p => p.ExchangeType = "fanout");

                    cfg.ConfigureEndpoints(context);
                });
        });

        services.Configure<MassTransitHostOptions>(options => options.StopTimeout = TimeSpan.FromSeconds(30));



        return services;
    }
}
