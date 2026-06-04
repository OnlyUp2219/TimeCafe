namespace Auth.TimeCafe.Application.CQRS.Events;

public class UserChangedEventHandler(HybridCache cache) : INotificationHandler<UserChangedEvent>
{
    public async Task Handle(UserChangedEvent notification, CancellationToken cancellationToken)
    {
        await cache.RemoveByTagAsync("users", cancellationToken);
        await cache.RemoveAsync(PermissionClaimsCacheKeys.UserPermissionsKey(notification.UserId), cancellationToken);
        await cache.RemoveByTagAsync(PermissionClaimsCacheKeys.UserPermissionsTag(notification.UserId), cancellationToken);
        await cache.RemoveByTagAsync($"user_{notification.UserId}", cancellationToken);
        await cache.RemoveByTagAsync("user_roles", cancellationToken);
    }
}

public class UserRolesChangedEventHandler(HybridCache cache) : INotificationHandler<UserRolesChangedEvent>
{
    public async Task Handle(UserRolesChangedEvent notification, CancellationToken cancellationToken)
    {
        await cache.RemoveByTagAsync("users", cancellationToken);
        await cache.RemoveAsync(PermissionClaimsCacheKeys.UserPermissionsKey(notification.UserId), cancellationToken);
        await cache.RemoveByTagAsync(PermissionClaimsCacheKeys.UserPermissionsTag(notification.UserId), cancellationToken);
        await cache.RemoveByTagAsync($"user_{notification.UserId}", cancellationToken);
        await cache.RemoveByTagAsync("user_roles", cancellationToken);
        await cache.RemoveByTagAsync(PermissionClaimsCacheKeys.RolePermissionsTag(notification.RoleName), cancellationToken);
    }
}

public class RoleClaimsChangedEventHandler(HybridCache cache) : INotificationHandler<RoleClaimsChangedEvent>
{
    public async Task Handle(RoleClaimsChangedEvent notification, CancellationToken cancellationToken)
    {
        await cache.RemoveByTagAsync(PermissionClaimsCacheKeys.RolePermissionsTag(notification.RoleName), cancellationToken);
        await cache.RemoveByTagAsync(PermissionClaimsCacheKeys.PermissionsTag, cancellationToken);
    }
}
