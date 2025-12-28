using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BuildingBlocks.Authorization;

/// <summary>
/// Обработчик авторизации для проверки разрешений через IPermissionService.
/// </summary>
public class PermissionAuthorizationHandler(IPermissionService permissionService)
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService = permissionService;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return; // Не авторизован — fail
        }

        var hasPermission = requirement.Mode switch
        {
            PermissionCheckMode.Any => await _permissionService.HasAnyPermissionAsync(userId, requirement.Permissions),
            PermissionCheckMode.All => await _permissionService.HasAllPermissionsAsync(userId, requirement.Permissions),
            _ => false
        };

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
