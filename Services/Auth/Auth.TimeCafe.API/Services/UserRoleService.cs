using Microsoft.AspNetCore.Identity;
using Auth.TimeCafe.Core.Services;

namespace Auth.TimeCafe.API.Services;

public class UserRoleService : IUserRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    public static readonly string AdminRole = "admin";
    public static readonly string ClientRole = "client";

    public UserRoleService(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task EnsureRolesCreatedAsync()
    {
        foreach (var roleName in new[] { AdminRole, ClientRole })
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    public async Task AssignRoleAsync(IdentityUser user, string role)
    {
        if (!await _userManager.IsInRoleAsync(user, role))
            await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<bool> IsUserInRoleAsync(IdentityUser user, string role)
        => await _userManager.IsInRoleAsync(user, role);

    public async Task<IList<string>> GetUserRolesAsync(IdentityUser user)
        => await _userManager.GetRolesAsync(user);
}
