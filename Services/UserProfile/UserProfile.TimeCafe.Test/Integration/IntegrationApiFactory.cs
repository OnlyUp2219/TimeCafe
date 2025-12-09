using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

using UserProfile.TimeCafe.Domain.DTOs;

namespace UserProfile.TimeCafe.Test.Integration;

public class IntegrationApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"UserProfileIntegrationDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Отключить автоматические миграции в тестах
        builder.UseSetting("SkipMigrations", "true");

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Kestrel:Endpoints:Https:Certificate:Path"] = string.Empty,
                ["Kestrel:Endpoints:Https:Certificate:Password"] = string.Empty,
                ["ConnectionStrings:DefaultConnection"] = string.Empty,
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
        });
    }
}
