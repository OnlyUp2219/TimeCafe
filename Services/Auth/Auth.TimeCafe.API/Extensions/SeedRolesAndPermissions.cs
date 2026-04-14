using System.Reflection;
using Microsoft.Extensions.Caching.Hybrid;

namespace Auth.TimeCafe.API.Extensions;

public static class SeedRolesAndPermissionsExtensions
{
    public static async Task SeedRolesAndPermissions(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var cache = scope.ServiceProvider.GetRequiredService<HybridCache>();

        var allPermissions = typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        await EnsureRoleClaimsAsync(roleManager, Roles.Admin, allPermissions);

        var clientPermissions = new List<string>
        {
            Permissions.AccountSelfRead,
            Permissions.AccountEmailChange,
            Permissions.AccountPasswordChange,
            Permissions.AccountPhoneSave,
            Permissions.AccountPhoneClear,
            Permissions.AccountPhoneGenerate,
            Permissions.AccountPhoneVerify,
            Permissions.AccountPhoneStatusRead,

            Permissions.UserProfileProfileCreate,
            Permissions.UserProfileProfileRead,
            Permissions.UserProfileProfileUpdate,
            Permissions.UserProfileAdditionalInfoCreate,
            Permissions.UserProfileAdditionalInfoRead,
            Permissions.UserProfileAdditionalInfoUpdate,
            Permissions.UserProfilePhotoCreate,
            Permissions.UserProfilePhotoRead,
            Permissions.UserProfilePhotoDelete,

            Permissions.BillingBalanceRead,
            Permissions.BillingDebtRead,
            Permissions.BillingTransactionRead,
            Permissions.BillingPaymentInitialize,
            Permissions.BillingPaymentHistoryRead,

            Permissions.VenueTariffRead,
            Permissions.VenuePromotionRead,
            Permissions.VenueThemeRead,
            Permissions.VenueVisitCreate,
            Permissions.VenueVisitRead,
            Permissions.VenueVisitEnd
        };

        await EnsureRoleClaimsAsync(roleManager, Roles.Client, clientPermissions);

        try
        {
            await cache.RemoveByTagAsync(BuildingBlocks.Permissions.PermissionClaimsCacheKeys.PermissionsTag);
        }
        catch
        {
        }
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
