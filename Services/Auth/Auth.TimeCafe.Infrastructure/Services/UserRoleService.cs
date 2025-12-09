namespace Auth.TimeCafe.Infrastructure.Services;

public class UserRoleService(RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager) : IUserRoleService
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public static readonly string AdminRole = "admin";
    public static readonly string ClientRole = "client";

    public async Task EnsureRolesCreatedAsync()
    {
        foreach (var roleName in new[] { AdminRole, ClientRole })
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }

    public async Task AssignRoleAsync(ApplicationUser user, string role)
    {
        if (!await _userManager.IsInRoleAsync(user, role))
            await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<bool> IsUserInRoleAsync(ApplicationUser user, string role)
        => await _userManager.IsInRoleAsync(user, role);

    public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        => await _userManager.GetRolesAsync(user);

    public async Task<ApplicationUser?> FindUserByIdAsync(Guid userId)
        => await _userManager.FindByIdAsync(userId.ToString());
}
