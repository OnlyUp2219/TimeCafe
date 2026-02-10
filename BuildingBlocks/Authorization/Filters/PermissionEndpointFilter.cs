using System.Security.Claims;

namespace BuildingBlocks.Authorization.Filters;

public class PermissionEndpointFilter : IEndpointFilter
{
    private readonly Permission[] _permissions;
    private readonly PermissionCheckMode _mode;

    public PermissionEndpointFilter(Permission permission)
    {
        _permissions = [permission];
        _mode = PermissionCheckMode.Any;
    }

    public PermissionEndpointFilter(PermissionCheckMode mode, params Permission[] permissions)
    {
        _permissions = permissions;
        _mode = mode;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var permissionService = httpContext.RequestServices.GetService<IPermissionService>();

        if (permissionService == null)
            return await next(context);

        var userIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdStr, out var userId))
            return Results.Unauthorized();

        var hasPermission = _mode switch
        {
            PermissionCheckMode.Any => await permissionService.HasAnyPermissionAsync(userId, _permissions),
            PermissionCheckMode.All => await permissionService.HasAllPermissionsAsync(userId, _permissions),
            _ => false
        };

        if (!hasPermission)
            return Results.Forbid();

        return await next(context);
    }
}
