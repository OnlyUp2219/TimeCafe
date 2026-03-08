
using Billing.TimeCafe.Test.Integration.Fakes;

namespace Billing.TimeCafe.Test.Integration;

public class IntegrationApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"BillingIntegrationDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("SkipMigrations", "true");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = string.Empty,
                ["CORS:PolicyName"] = "TestPolicy",
                ["Redis:ConnectionString"] = "inmemory",
                ["RabbitMQ:Host"] = "inmemory",
                ["RabbitMQ:Username"] = "test",
                ["RabbitMQ:Password"] = "test",
                ["Stripe:PublishableKey"] = "pk_test",
                ["Stripe:SecretKey"] = "sk_test",
                ["Stripe:WebhookSecret"] = string.Empty,
                ["Stripe:DefaultReturnUrl"] = "https://example.com/return",
                ["Jwt:SigningKey"] = "test-signing-key-minimum-32-characters-long-for-hmacsha256",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            };
            cfg.AddInMemoryCollection(overrides);
        });

        builder.ConfigureTestServices(services =>
        {
            ReplaceDbContext(services);
            ReplaceRedis(services);
            ReplaceMassTransit(services);
            ReplaceStripe(services);
        });
    }

    private void ReplaceDbContext(IServiceCollection services)
    {
        var configurationDescriptors = services
            .Where(d => d.ServiceType.Name.Contains("IDbContextOptionsConfiguration")
                        && d.ServiceType.IsGenericType
                        && d.ServiceType.GetGenericArguments().FirstOrDefault() == typeof(ApplicationDbContext))
            .ToList();

        foreach (var descriptor in configurationDescriptors)
        {
            services.Remove(descriptor);
        }

        var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
        if (dbContextDescriptor != null)
        {
            services.Remove(dbContextDescriptor);
        }

        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(_dbName));
    }

    private void ReplaceRedis(IServiceCollection services)
    {
        var cacheDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDistributedCache));
        if (cacheDescriptor != null)
        {
            services.Remove(cacheDescriptor);
        }

        var muxDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));
        if (muxDescriptor != null)
        {
            services.Remove(muxDescriptor);
        }

        services.AddDistributedMemoryCache();
    }

    private void ReplaceMassTransit(IServiceCollection services)
    {
        var descriptorsToRemove = services
            .Where(d => d.ServiceType.Namespace != null && d.ServiceType.Namespace.StartsWith("MassTransit"))
            .ToList();

        foreach (var descriptor in descriptorsToRemove)
        {
            services.Remove(descriptor);
        }

        var descriptorsToRemoveImpl = services
            .Where(d => d.ImplementationType != null
                        && d.ImplementationType.Namespace != null
                        && d.ImplementationType.Namespace.StartsWith("MassTransit"))
            .ToList();

        foreach (var descriptor in descriptorsToRemoveImpl)
        {
            services.Remove(descriptor);
        }

        services.AddMassTransit(cfg =>
        {
            cfg.AddBillingMassTransit();
            cfg.UsingInMemory((context, busCfg) =>
            {
                busCfg.ConfigureEndpoints(context);
            });
        });
    }

    private void ReplaceStripe(IServiceCollection services)
    {
        var stripeDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStripePaymentClient));
        if (stripeDescriptor != null)
        {
            services.Remove(stripeDescriptor);
        }

        services.AddSingleton<IStripePaymentClient, FakeStripePaymentClient>();
    }
}
