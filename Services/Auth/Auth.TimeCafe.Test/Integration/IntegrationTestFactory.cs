namespace Auth.TimeCafe.Test.Integration;

public class IntegrationTestFactory
{
    public ServiceProvider Services { get; }

    public IntegrationTestFactory()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("IntegrationTestDb"));

        serviceCollection.AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        serviceCollection.AddScoped<IUserRoleService, UserRoleService>();
        serviceCollection.AddScoped<IPermissionService, PermissionService>();

        Services = serviceCollection.BuildServiceProvider();
    }
}
