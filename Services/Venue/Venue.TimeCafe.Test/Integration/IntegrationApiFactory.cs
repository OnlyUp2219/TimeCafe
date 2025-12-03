using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace Venue.TimeCafe.Test.Integration;

public class IntegrationApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"VenueIntegrationDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("SkipMigrations", "true");

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = string.Empty,
                ["CORS:PolicyName"] = "TestPolicy"
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
        });
    }
}
