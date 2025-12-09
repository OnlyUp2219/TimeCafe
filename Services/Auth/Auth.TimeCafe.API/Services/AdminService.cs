namespace Auth.TimeCafe.API.Services;

public class AdminService(UserManager<ApplicationUser> userManager, IUserRoleService roleService)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IUserRoleService _roleService = roleService;

    public async Task<(bool Success, IEnumerable<IdentityError> Errors)> CreateAdminAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return (false, result.Errors);
        }

        await _roleService.AssignRoleAsync(user, UserRoleService.AdminRole);
        return (true, Array.Empty<IdentityError>());
    }
}
