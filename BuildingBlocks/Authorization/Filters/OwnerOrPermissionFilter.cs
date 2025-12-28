using System.Security.Claims;

namespace BuildingBlocks.Authorization.Filters;

/// <summary>
/// IDOR защита: пользователь владелец ресурса ИЛИ имеет разрешение (админ).
/// </summary>
public class OwnerOrPermissionFilter : IEndpointFilter
{
    private readonly string _userIdParameterName;
    private readonly Permission[] _fallbackPermissions;
    private readonly PermissionCheckMode _mode;

    public OwnerOrPermissionFilter(string userIdParameterName, Permission fallbackPermission)
    {
        _userIdParameterName = userIdParameterName;
        _fallbackPermissions = [fallbackPermission];
        _mode = PermissionCheckMode.Any;
    }

    public OwnerOrPermissionFilter(
        string userIdParameterName,
        PermissionCheckMode mode,
        params Permission[] fallbackPermissions)
    {
        _userIdParameterName = userIdParameterName;
        _fallbackPermissions = fallbackPermissions;
        _mode = mode;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        var currentUserIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
            return Results.Unauthorized();

        var requestedUserId = ExtractUserIdFromRequest(context);

        // Свой ресурс — пропускаем
        if (requestedUserId.HasValue && requestedUserId.Value == currentUserId)
            return await next(context);

        // Проверяем разрешение (для чужих данных)
        var permissionService = httpContext.RequestServices.GetService<IPermissionService>();

        if (permissionService == null)
            return Results.Forbid();

        var hasPermission = _mode switch
        {
            PermissionCheckMode.Any => await permissionService.HasAnyPermissionAsync(currentUserId, _fallbackPermissions),
            PermissionCheckMode.All => await permissionService.HasAllPermissionsAsync(currentUserId, _fallbackPermissions),
            _ => false
        };

        if (!hasPermission)
            return Results.Forbid();

        return await next(context);
    }

    private Guid? ExtractUserIdFromRequest(EndpointFilterInvocationContext context)
    {
        if (context.HttpContext.Request.RouteValues.TryGetValue(_userIdParameterName, out var routeValue))
        {
            if (Guid.TryParse(routeValue?.ToString(), out var routeUserId))
                return routeUserId;
        }

        var queryValue = context.HttpContext.Request.Query[_userIdParameterName].FirstOrDefault();
        if (Guid.TryParse(queryValue, out var queryUserId))
            return queryUserId;

        foreach (var arg in context.Arguments)
        {
            if (arg == null) continue;

            if (arg is Guid guidArg)
                return guidArg;

            var userIdProperty = arg.GetType().GetProperty("UserId")
                              ?? arg.GetType().GetProperty(_userIdParameterName);

            if (userIdProperty?.GetValue(arg) is Guid propValue)
                return propValue;
        }

        return null;
    }
}
