namespace Auth.TimeCafe.Infrastructure.Permissions;

public class PermissionService(IUserRoleService userRoleService) : IPermissionService
{
    private readonly IUserRoleService _userRoleService = userRoleService;
    private static readonly Dictionary<string, Permission[]> RolePermissions = new()
    {
        { UserRoleService.AdminRole, new[] {
            Permission.AdminView, Permission.AdminEdit, Permission.AdminDelete, Permission.AdminCreate,
            Permission.ClientView, Permission.ClientEdit, Permission.ClientDelete, Permission.ClientCreate }
        },
        { UserRoleService.ClientRole, new[] {
            Permission.ClientView, Permission.ClientEdit, Permission.ClientDelete, Permission.ClientCreate}
        }
    };

    public async Task<bool> HasPermissionAsync(string userId, Permission permission)
    {
        var user = await _userRoleService.FindUserByIdAsync(userId);
        if (user == null) return false;
        var roles = await _userRoleService.GetUserRolesAsync(user);
        return roles.Any(role => RolePermissions.TryGetValue(role, out var perms) && perms.Contains(permission));
    }
}
