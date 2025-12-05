using Auth.TimeCafe.Application.Contracts;

namespace Auth.TimeCafe.Test.Integration;

public class IntegrationTestFactory
{
    public ServiceProvider Services { get; }

    public IntegrationTestFactory()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("IntegrationTestDb"));

        serviceCollection.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        var inMemoryConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "test_signing_key_12345678901234567890",
                ["Jwt:AccessTokenExpirationMinutes"] = "15",
                ["Jwt:RefreshTokenExpirationDays"] = "7",
                ["Jwt:Issuer"] = "TimeCafe.Test",
                ["Jwt:Audience"] = "TimeCafe.Test.Client"
            })
            .Build();
        serviceCollection.AddSingleton<IConfiguration>(inMemoryConfig);

        serviceCollection.AddScoped<IUserRoleService, UserRoleService>();
        serviceCollection.AddScoped<IPermissionService, PermissionService>();
        serviceCollection.AddScoped<IJwtService, JwtService>();

        Services = serviceCollection.BuildServiceProvider();
    }
}
