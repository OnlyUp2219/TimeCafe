namespace Auth.TimeCafe.Infrastructure.Services;

public static class PermissionClaimsCacheKeys
{
    public const string AllPermissionsTag = "auth.permissions";

    public static string UserPermissionsTag(Guid userId) => $"auth.permissions.user:{userId:N}";

    public static string UserPermissionsKey(Guid userId) => $"auth.permissions.snapshot:{userId:N}";
}