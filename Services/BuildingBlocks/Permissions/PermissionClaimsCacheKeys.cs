namespace BuildingBlocks.Permissions;

public static class PermissionClaimsCacheKeys
{
    public const string PermissionsTag = "auth.permissions";

    public static string UserPermissionsTag(Guid userId) => $"auth.permissions.user:{userId:N}";

    public static string RolePermissionsTag(string roleName) => $"auth.permissions.role:{roleName.Trim().ToUpperInvariant()}";

    public static string UserPermissionsKey(Guid userId) => $"auth.permissions.snapshot:{userId:N}";
}
