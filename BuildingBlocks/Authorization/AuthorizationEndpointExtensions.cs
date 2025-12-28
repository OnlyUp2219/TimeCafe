using BuildingBlocks.Authorization.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.Authorization;

public static class AuthorizationEndpointExtensions
{
    /// <summary>
    /// Требует наличие одного из указанных разрешений.
    /// </summary>
    public static RouteHandlerBuilder RequirePermission(
        this RouteHandlerBuilder builder,
        params Permission[] permissions)
    {
        return builder.AddEndpointFilter(
            new PermissionEndpointFilter(PermissionCheckMode.Any, permissions));
    }

    /// <summary>
    /// Требует наличие ВСЕХ указанных разрешений.
    /// </summary>
    public static RouteHandlerBuilder RequireAllPermissions(
        this RouteHandlerBuilder builder,
        params Permission[] permissions)
    {
        return builder.AddEndpointFilter(
            new PermissionEndpointFilter(PermissionCheckMode.All, permissions));
    }

    /// <summary>
    /// IDOR защита: доступ к своему ресурсу ИЛИ с нужным разрешением (админ).
    /// fallbackPermission — обычно AdminView/AdminEdit для доступа к чужим данным.
    /// </summary>
    public static RouteHandlerBuilder RequireOwnerOrPermission(
        this RouteHandlerBuilder builder,
        string userIdParameterName,
        Permission fallbackPermission)
    {
        return builder.AddEndpointFilter(
            new OwnerOrPermissionFilter(userIdParameterName, fallbackPermission));
    }

    public static RouteHandlerBuilder RequireOwnerOrAnyPermission(
        this RouteHandlerBuilder builder,
        string userIdParameterName,
        params Permission[] fallbackPermissions)
    {
        return builder.AddEndpointFilter(
            new OwnerOrPermissionFilter(userIdParameterName, PermissionCheckMode.Any, fallbackPermissions));
    }

    public static RouteHandlerBuilder RequireOwnerOrAllPermissions(
        this RouteHandlerBuilder builder,
        string userIdParameterName,
        params Permission[] fallbackPermissions)
    {
        return builder.AddEndpointFilter(
            new OwnerOrPermissionFilter(userIdParameterName, PermissionCheckMode.All, fallbackPermissions));
    }

    public static RouteHandlerBuilder RequirePermissionPolicy(
        this RouteHandlerBuilder builder,
        Permission permission)
    {
        return builder.RequireAuthorization($"Permission:{permission}");
    }
}
