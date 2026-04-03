using System.Reflection;

namespace Auth.TimeCafe.API.Extensions;

public static class SeedRolesAndPermissionsExtensions
{
    public static async Task SeedRolesAndPermissions(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var adminRole = await roleManager.FindByNameAsync(Roles.Admin);
        if (adminRole is null)
        {
            adminRole = new IdentityRole<Guid>(Roles.Admin);
            await roleManager.CreateAsync(adminRole);

            var allPermissions = typeof(Permissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null))
                .ToList();

            foreach (var permissions in allPermissions)
            {
                await roleManager.AddClaimAsync(adminRole, new Claim(CustomClaimTypes.Permissions, permissions!));
            }
        }

        var clientRole = await roleManager.FindByNameAsync(Roles.Client);
        if (clientRole is null)
        {
            clientRole = new IdentityRole<Guid>(Roles.Client);
            await roleManager.CreateAsync(clientRole);

            await roleManager.AddClaimAsync(clientRole, new Claim(CustomClaimTypes.Permissions, Permissions.ClientCreate));
            await roleManager.AddClaimAsync(clientRole, new Claim(CustomClaimTypes.Permissions, Permissions.ClientRead));
            await roleManager.AddClaimAsync(clientRole, new Claim(CustomClaimTypes.Permissions, Permissions.ClientUpdate));
            await roleManager.AddClaimAsync(clientRole, new Claim(CustomClaimTypes.Permissions, Permissions.ClientDelete));
        }
    }

}
