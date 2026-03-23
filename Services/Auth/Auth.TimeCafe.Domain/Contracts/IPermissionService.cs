namespace Auth.TimeCafe.Domain.Contracts;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, Permission permission);
}
