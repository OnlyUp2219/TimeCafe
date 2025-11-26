namespace Auth.TimeCafe.Test.Integration;

public class PermissionsAndRolesTests
{
    private readonly ServiceProvider _services;

    public PermissionsAndRolesTests()
    {
        var factory = new IntegrationTestFactory();
        _services = factory.Services;
        SeedUsersAndRoles();
    }

    private void SeedUsersAndRoles()
    {
        var userManager = _services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = _services.GetRequiredService<RoleManager<IdentityRole>>();

        // Создаём роли
        if (!roleManager.RoleExistsAsync("admin").Result)
            roleManager.CreateAsync(new IdentityRole("admin")).Wait();
        if (!roleManager.RoleExistsAsync("client").Result)
            roleManager.CreateAsync(new IdentityRole("client")).Wait();

        // Создаём пользователей
        var admin = userManager.FindByEmailAsync("admin@timecafe.local").Result;
        if (admin == null)
        {
            admin = new IdentityUser { UserName = "admin", Email = "admin@timecafe.local", EmailConfirmed = true };
            userManager.CreateAsync(admin, "P@ssw0rd!").Wait();
        }
        if (!userManager.IsInRoleAsync(admin, "admin").Result)
            userManager.AddToRoleAsync(admin, "admin").Wait();

        var client = userManager.FindByEmailAsync("client@timecafe.local").Result;
        if (client == null)
        {
            client = new IdentityUser { UserName = "client", Email = "client@timecafe.local", EmailConfirmed = true };
            userManager.CreateAsync(client, "P@ssw0rd!").Wait();
        }
        if (!userManager.IsInRoleAsync(client, "client").Result)
            userManager.AddToRoleAsync(client, "client").Wait();
    }

    [Fact]
    public async Task Admin_Should_Have_All_Permissions()
    {
        var userManager = _services.GetRequiredService<UserManager<IdentityUser>>();
        var permissionService = _services.GetRequiredService<IPermissionService>();
        var admin = await userManager.FindByEmailAsync("admin@timecafe.local");
        admin.Should().NotBeNull();

        foreach (var permission in Enum.GetValues(typeof(Permission)).Cast<Permission>())
        {
            var hasPermission = await permissionService.HasPermissionAsync(admin.Id, permission);
            hasPermission.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Client_Should_Have_Only_Client_Permissions()
    {
        var userManager = _services.GetRequiredService<UserManager<IdentityUser>>();
        var permissionService = _services.GetRequiredService<IPermissionService>();
        var client = await userManager.FindByEmailAsync("client@timecafe.local");
        client.Should().NotBeNull();

        var allowed = new[] { Permission.ClientEdit, Permission.ClientView, Permission.ClientDelete, Permission.ClientCreate };
        foreach (var permission in allowed)
        {
            var hasPermission = await permissionService.HasPermissionAsync(client.Id, permission);
            hasPermission.Should().BeTrue();
        }
        var forbidden = Enum.GetValues(typeof(Permission)).Cast<Permission>().Except(allowed);
        foreach (var permission in forbidden)
        {
            var hasPermission = await permissionService.HasPermissionAsync(client.Id, permission);
            hasPermission.Should().BeFalse();
        }
    }

    [Fact]
    public async Task Role_Check_Should_Work()
    {
        var userManager = _services.GetRequiredService<UserManager<IdentityUser>>();
        var userRoleService = _services.GetRequiredService<IUserRoleService>();
        var admin = await userManager.FindByEmailAsync("admin@timecafe.local");
        var client = await userManager.FindByEmailAsync("client@timecafe.local");
        admin.Should().NotBeNull();
        client.Should().NotBeNull();

        var adminRoles = await userRoleService.GetUserRolesAsync(admin);
        adminRoles.Should().Contain("admin");
        var clientRoles = await userRoleService.GetUserRolesAsync(client);
        clientRoles.Should().Contain("client");
    }
}
