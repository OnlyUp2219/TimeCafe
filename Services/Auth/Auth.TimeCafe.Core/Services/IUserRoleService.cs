using Microsoft.AspNetCore.Identity;

namespace Auth.TimeCafe.Core.Services;

public interface IUserRoleService
{
    Task EnsureRolesCreatedAsync();
    Task AssignRoleAsync(IdentityUser user, string role);
    Task<bool> IsUserInRoleAsync(IdentityUser user, string role);
    Task<IList<string>> GetUserRolesAsync(IdentityUser user);
}
