using System.Reflection;

namespace Auth.TimeCafe.API.Extensions;

public static class SeedRolesAndPermissionsExtensions
{
    public static async Task SeedRolesAndPermissions(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var allPermissions = typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        await EnsureRoleClaimsAsync(roleManager, Roles.Admin, allPermissions);

        var clientPermissions = new List<string>
        {
            Permissions.AccountSelfRead
        };

        await EnsureRoleClaimsAsync(roleManager, Roles.Client, clientPermissions);
    }

    private static async Task EnsureRoleClaimsAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        string roleName,
        IReadOnlyCollection<string> targetPermissions)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            role = new IdentityRole<Guid>(roleName);
            await roleManager.CreateAsync(role);
        }

        var existingClaims = await roleManager.GetClaimsAsync(role);
        var existingPermissions = existingClaims
            .Where(c => c.Type == CustomClaimTypes.Permissions)
            .Select(c => c.Value)
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var claim in existingClaims.Where(c => c.Type == CustomClaimTypes.Permissions && !targetPermissions.Contains(c.Value, StringComparer.Ordinal)))
        {
            await roleManager.RemoveClaimAsync(role, claim);
        }

        foreach (var permission in targetPermissions.Where(p => !existingPermissions.Contains(p)))
        {
            await roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permissions, permission));
        }
    }

}
