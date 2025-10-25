namespace Auth.TimeCafe.Domain.Services;

public interface IUserRoleService
{
    Task EnsureRolesCreatedAsync();
    Task AssignRoleAsync(IdentityUser user, string role);
    Task<bool> IsUserInRoleAsync(IdentityUser user, string role);
    Task<IList<string>> GetUserRolesAsync(IdentityUser user);
}
