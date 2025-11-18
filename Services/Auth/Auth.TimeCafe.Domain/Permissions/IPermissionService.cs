namespace Auth.TimeCafe.Domain.Permissions;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string userId, Permission permission);
}
