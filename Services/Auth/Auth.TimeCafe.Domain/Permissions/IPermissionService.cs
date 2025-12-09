namespace Auth.TimeCafe.Domain.Permissions;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, Permission permission);
}
