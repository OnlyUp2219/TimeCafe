using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Venue.TimeCafe.Test.Integration.Mocks;

namespace Venue.TimeCafe.Test.Integration;

public class IntegrationApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"VenueIntegrationDb_{Guid.NewGuid()}";
    private static readonly SemaphoreSlim InfraLock = new(1, 1);
    private static bool _infraInitialized;
    private static string _redisConnectionString = "127.0.0.1:6379";
    private static string _rabbitHost = "localhost";
    private static int _rabbitPort = 5672;
    private const string RabbitUser = "guest";
    private const string RabbitPassword = "guest";
    private static RedisContainer? _redisContainer;
    private static RabbitMqContainer? _rabbitContainer;

    public static string? InfrastructureUnavailableReason { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        EnsureInfrastructure();

        builder.UseEnvironment("Testing");

        builder.UseSetting("SkipMigrations", "true");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = string.Empty,
                ["CORS:PolicyName"] = "TestPolicy",
                ["Jwt:SigningKey"] = "test-signing-key-minimum-32-characters-long-for-hmacsha256",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Redis:ConnectionString"] = _redisConnectionString,
                ["RabbitMQ:Host"] = _rabbitHost,
                ["RabbitMQ:Port"] = _rabbitPort.ToString(),
                ["RabbitMQ:Username"] = RabbitUser,
                ["RabbitMQ:Password"] = RabbitPassword,
                ["IntegrationTests:UseRealInfrastructure"] = "true"
            };
            cfg.AddInMemoryCollection(overrides);
        });

        builder.ConfigureTestServices(services =>
        {
            ReplaceMassTransit(services);

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

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            ReplaceRedis(services);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, _ => { });

            services.AddAuthorization();

            // Mock IVisitBalancePolicyService for integration tests
            var balancePolicyDescriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IVisitBalancePolicyService));
            if (balancePolicyDescriptor != null)
            {
                services.Remove(balancePolicyDescriptor);
            }
            services.AddScoped<IVisitBalancePolicyService, MockVisitBalancePolicyService>();
        });
    }

    private static void ReplaceMassTransit(IServiceCollection services)
    {
        var descriptorsToRemoveByService = services
            .Where(d => d.ServiceType.Namespace?.StartsWith("MassTransit") == true)
            .ToList();

        foreach (var descriptor in descriptorsToRemoveByService)
        {
            services.Remove(descriptor);
        }

        var descriptorsToRemoveByImplementation = services
            .Where(d => d.ImplementationType != null
                        && d.ImplementationType.Namespace?.StartsWith("MassTransit") == true)
            .ToList();

        foreach (var descriptor in descriptorsToRemoveByImplementation)
        {
            services.Remove(descriptor);
        }

        services.AddMassTransit(cfg => cfg.UsingInMemory((context, busCfg) => busCfg.ConfigureEndpoints(context)));
    }

    private static void ReplaceRedis(IServiceCollection services)
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

        services.AddStackExchangeRedisCache(options => options.Configuration = _redisConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(_redisConnectionString));
    }

    private static void EnsureInfrastructure()
    {
        if (_infraInitialized)
            return;

        InfraLock.Wait();
        try
        {
            if (_infraInitialized)
                return;

            try
            {
                _redisContainer = new RedisBuilder().Build();
                _rabbitContainer = new RabbitMqBuilder()
                    .WithUsername(RabbitUser)
                    .WithPassword(RabbitPassword)
                    .Build();

                _redisContainer.StartAsync().GetAwaiter().GetResult();
                _rabbitContainer.StartAsync().GetAwaiter().GetResult();

                _redisConnectionString = $"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}";
                _rabbitHost = _rabbitContainer.Hostname;
                _rabbitPort = _rabbitContainer.GetMappedPublicPort(5672);
                InfrastructureUnavailableReason = null;
            }
            catch (Exception ex)
            {
                InfrastructureUnavailableReason = $"Docker контейнер не запущен или недоступен. Запустите Docker и повторите тесты. Ошибка: {ex.Message}";
            }

            _infraInitialized = true;
        }
        finally
        {
            InfraLock.Release();
        }
    }
}







