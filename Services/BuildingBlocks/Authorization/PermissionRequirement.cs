using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public Permission[] Permissions { get; }
    public PermissionCheckMode Mode { get; }

    public PermissionRequirement(Permission permission)
    {
        Permissions = [permission];
        Mode = PermissionCheckMode.Any;
    }

    public PermissionRequirement(PermissionCheckMode mode, params Permission[] permissions)
    {
        Permissions = permissions;
        Mode = mode;
    }
}

public enum PermissionCheckMode
{
    Any,
    All
}
