namespace Auth.TimeCafe.Application.Contracts;

public interface IRbacRepository
{
    List<string> GetPermissions();
    Task<List<RoleClaimsResponse>> GetRoleClaimsAsync();
    Task<List<RolesResponse>> GetRolesAsync();

    Task<bool> RoleExistsAsync(string roleName);

    Task<Result> CreateRoleClaimsAsync(string roleName, List<string> claims);
    Task<Result> UpdateRoleClaimsAsync(string roleName, List<string> newClaims);
    Task<Result> DeleteRoleAsync(string roleName);
    Task<Result> UpdateRoleNameAsync(string oldRoleName, string newRoleName);
}
