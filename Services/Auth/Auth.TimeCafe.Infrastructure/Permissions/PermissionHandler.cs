using System.Security.Claims;

namespace Auth.TimeCafe.Infrastructure.Permissions;

public class PermissionHandler(IPermissionService permissionService) : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService = permissionService;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out var userId) && await _permissionService.HasPermissionAsync(userId, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
