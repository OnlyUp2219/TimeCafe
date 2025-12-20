
namespace Auth.TimeCafe.Domain.Contracts;

public interface IUserRoleService
{
    Task EnsureRolesCreatedAsync();
    Task AssignRoleAsync(ApplicationUser user, string role);
    Task<bool> IsUserInRoleAsync(ApplicationUser user, string role);
    Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
    Task<ApplicationUser?> FindUserByIdAsync(Guid userId);
}
