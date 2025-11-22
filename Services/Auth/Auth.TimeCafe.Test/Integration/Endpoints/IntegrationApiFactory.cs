namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class IntegrationApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"IntegrationTestsDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Kestrel:Endpoints:Https:Certificate:Path"] = string.Empty,
                ["Kestrel:Endpoints:Https:Certificate:Password"] = string.Empty,
                ["ConnectionStrings:DefaultConnection"] = string.Empty,
                ["Seed:Admin:Email"] = "test@admin.com",
                ["Seed:Admin:Password"] = "TestP@ssw0rd!",
                ["Seed:Admin:Role"] = "admin"
            };
            cfg.AddInMemoryCollection(overrides);
        });

        builder.ConfigureTestServices(services =>
        {
            var dbContextOptionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));

            if (dbContextOptionsDescriptor != null)
            {
                services.Remove(dbContextOptionsDescriptor);
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
