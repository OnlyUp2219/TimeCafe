namespace BuildingBlocks.Authorization;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, Permission permission);
    Task<bool> HasAnyPermissionAsync(Guid userId, params Permission[] permissions);
    Task<bool> HasAllPermissionsAsync(Guid userId, params Permission[] permissions);
}
