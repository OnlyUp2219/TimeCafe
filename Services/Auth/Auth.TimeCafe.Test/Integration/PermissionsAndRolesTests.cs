using Auth.TimeCafe.Application.Contracts;

namespace Auth.TimeCafe.Test.Integration;

public class PermissionsAndRolesTests
{
    private readonly ServiceProvider _services;

    public PermissionsAndRolesTests()
    {
        var factory = new IntegrationTestFactory();
        _services = factory.Services;
        SeedUsersAndRolesAsync().GetAwaiter().GetResult();
    }

    private async Task SeedUsersAndRolesAsync()
    {
        var userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = _services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!await roleManager.RoleExistsAsync("admin"))
            await roleManager.CreateAsync(new IdentityRole<Guid>("admin"));
        if (!await roleManager.RoleExistsAsync("client"))
            await roleManager.CreateAsync(new IdentityRole<Guid>("client"));

        var admin = await userManager.FindByEmailAsync("admin@timecafe.local");
        if (admin == null)
        {
            admin = new ApplicationUser { UserName = "admin", Email = "admin@timecafe.local", EmailConfirmed = true };
            await userManager.CreateAsync(admin, "P@ssw0rd!");
        }
        if (!await userManager.IsInRoleAsync(admin, "admin"))
            await userManager.AddToRoleAsync(admin, "admin");

        var client = await userManager.FindByEmailAsync("client@timecafe.local");
        if (client == null)
        {
            client = new ApplicationUser { UserName = "client", Email = "client@timecafe.local", EmailConfirmed = true };
            await userManager.CreateAsync(client, "P@ssw0rd!");
        }
        if (!await userManager.IsInRoleAsync(client, "client"))
            await userManager.AddToRoleAsync(client, "client");
    }

    [Fact]
    public async Task Admin_Should_Have_All_Permissions()
    {
        var userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
        var jwt = _services.GetRequiredService<IJwtService>();
        var permissionService = _services.GetRequiredService<IPermissionService>();
        var admin = await userManager.FindByEmailAsync("admin@timecafe.local");
        admin.Should().NotBeNull();

        var tokensRevoked = await jwt.RevokeUserTokensAsync(admin.Id, CancellationToken.None);

        foreach (var permission in Enum.GetValues(typeof(Permission)).Cast<Permission>())
        {
            var hasPermission = await permissionService.HasPermissionAsync(admin.Id, permission);
            hasPermission.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Client_Should_Have_Only_Client_Permissions()
    {
        var userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
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
        var userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
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
