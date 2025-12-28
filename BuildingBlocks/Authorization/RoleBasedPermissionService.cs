namespace BuildingBlocks.Authorization;

/// <summary>
/// Базовая реализация IPermissionService на основе ролей.
/// Наследуйте и переопределите GetUserRolesAsync.
/// </summary>
public abstract class RoleBasedPermissionService : IPermissionService
{
    public static class Roles
    {
        public const string Admin = "admin";
        public const string Client = "client";
    }

    protected virtual Dictionary<string, Permission[]> RolePermissions => new()
    {
        {
            Roles.Admin,
            [
                Permission.AdminView, Permission.AdminEdit, Permission.AdminDelete, Permission.AdminCreate,
                Permission.ClientView, Permission.ClientEdit, Permission.ClientDelete, Permission.ClientCreate
            ]
        },
        {
            Roles.Client,
            [
                Permission.ClientView, Permission.ClientEdit, Permission.ClientDelete, Permission.ClientCreate
            ]
        }
    };

    protected abstract Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);

    public async Task<bool> HasPermissionAsync(Guid userId, Permission permission)
    {
        var roles = await GetUserRolesAsync(userId);
        return roles.Any(role =>
            RolePermissions.TryGetValue(role, out var perms) && perms.Contains(permission));
    }

    public async Task<bool> HasAnyPermissionAsync(Guid userId, params Permission[] permissions)
    {
        var roles = await GetUserRolesAsync(userId);
        return roles.Any(role =>
            RolePermissions.TryGetValue(role, out var perms) &&
            permissions.Any(p => perms.Contains(p)));
    }

    public async Task<bool> HasAllPermissionsAsync(Guid userId, params Permission[] permissions)
    {
        var roles = await GetUserRolesAsync(userId);
        var userPermissions = roles
            .Where(role => RolePermissions.ContainsKey(role))
            .SelectMany(role => RolePermissions[role])
            .Distinct()
            .ToHashSet();

        return permissions.All(p => userPermissions.Contains(p));
    }
}
