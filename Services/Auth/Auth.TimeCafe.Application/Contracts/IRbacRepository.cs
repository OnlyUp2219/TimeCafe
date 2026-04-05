namespace Auth.TimeCafe.Application.Contracts;

public interface IRbacRepository
{
    List<string> GetPermissions();
    Task<List<RoleClaimsResponse>> GetRoleClaimsAsync();
    Task<List<RolesResponse>> GetRolesAsync();

    Task<bool> RoleExistsAsync(string RoleName);

    Task<Result> CreateRoleClaimsAsync(string RoleName, List<string> Claims);
    Task<Result> UpdateRoleClaimsAsync(string RoleName, List<string> NewClaims);
    Task<Result> DeleteRoleAsync(string RoleName);
    Task<Result> UpdateRoleNameAsync(string OldRoleName, string NewRoleName);

}
