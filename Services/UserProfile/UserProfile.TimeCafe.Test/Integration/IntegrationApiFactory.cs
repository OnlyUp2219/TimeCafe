using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

using UserProfile.TimeCafe.Domain.DTOs;

namespace UserProfile.TimeCafe.Test.Integration;

public class IntegrationApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"UserProfileIntegrationDb_{Guid.NewGuid()}";
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

        // Отключить автоматические миграции в тестах
        builder.UseSetting("SkipMigrations", "true");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Kestrel:Endpoints:Https:Certificate:Path"] = string.Empty,
                ["Kestrel:Endpoints:Https:Certificate:Password"] = string.Empty,
                ["ConnectionStrings:DefaultConnection"] = string.Empty,
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

            // Mock photo moderation service for tests (always returns safe)
            var moderationDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPhotoModerationService));
            if (moderationDescriptor != null)
            {
                services.Remove(moderationDescriptor);
            }
            services.AddScoped<IPhotoModerationService>(_ =>
            {
                var mock = new Mock<IPhotoModerationService>();
                mock.Setup(m => m.ModeratePhotoAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ModerationResult(true, null, null));
                return mock.Object;
            });


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, _ => { });
        });
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
