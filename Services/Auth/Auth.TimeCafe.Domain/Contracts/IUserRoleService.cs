namespace Auth.TimeCafe.Domain.Contracts;

public interface IUserRoleService
{
    Task EnsureRolesCreatedAsync();
    Task AssignRoleAsync(IdentityUser user, string role);
    Task<bool> IsUserInRoleAsync(IdentityUser user, string role);
    Task<IList<string>> GetUserRolesAsync(IdentityUser user);
    Task<IdentityUser?> FindUserByIdAsync(string userId);
}
