namespace Auth.TimeCafe.Infrastructure.Services;

public class RbacRepository : IRbacRepository
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public RbacRepository(
        ApplicationDbContext context,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
    }

    public List<string> GetPermissions()
    {
        return _context.RoleClaims
            .Where(roleClaim =>
                roleClaim.ClaimType == CustomClaimTypes.Permissions &&
                roleClaim.ClaimValue != null)
            .Select(roleClaim => roleClaim.ClaimValue!)
            .Distinct()
            .ToList();
    }

    public async Task<List<RoleClaimsResponse>> GetRoleClaimsAsync()
    {
        return await _context.Roles
            .Select(role => new RoleClaimsResponse
            {
                RoleId = role.Id,
                RoleName = role.Name!,
                Claims = _context.RoleClaims
                    .Where(roleClaim =>
                        roleClaim.RoleId == role.Id &&
                        roleClaim.ClaimType == CustomClaimTypes.Permissions &&
                        roleClaim.ClaimValue != null)
                    .Select(roleClaim => roleClaim.ClaimValue!)
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<List<RolesResponse>> GetRolesAsync()
    {
        return await _context.Roles
            .Select(role => new RolesResponse
            {
                RoleId = role.Id,
                RoleName = role.Name!
            })
            .ToListAsync();
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _roleManager.RoleExistsAsync(roleName);
    }

    public async Task<Result> CreateRoleClaimsAsync(string roleName, List<string> claims)
    {
        var uniqueClaims = NormalizeClaims(claims);

        if (uniqueClaims.Count == 0)
            return Result.Fail("Список разрешений не должен быть пустым.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var role = new IdentityRole<Guid>(roleName);
            var identityResult = await _roleManager.CreateAsync(role);

            if (!identityResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return Result.Fail(identityResult.Errors.Select(error => error.Description));
            }

            var claimsToAdd = uniqueClaims.Select(claim => new IdentityRoleClaim<Guid>
            {
                RoleId = role.Id,
                ClaimType = CustomClaimTypes.Permissions,
                ClaimValue = claim
            });

            await _context.RoleClaims.AddRangeAsync(claimsToAdd);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Ok();
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();
            return Result.Fail(exception.Message);
        }
    }

    public async Task<Result> UpdateRoleClaimsAsync(string roleName, List<string> newClaims)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role is null)
            return Result.Fail(new RoleNotFoundError(roleName));

        var uniqueClaims = NormalizeClaims(newClaims);
        if (uniqueClaims.Count == 0)
            return Result.Fail("Список разрешений не должен быть пустым.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var oldClaims = _context.RoleClaims.Where(roleClaim =>
                roleClaim.RoleId == role.Id &&
                roleClaim.ClaimType == CustomClaimTypes.Permissions);

            _context.RoleClaims.RemoveRange(oldClaims);

            var claimsToAdd = uniqueClaims.Select(claim => new IdentityRoleClaim<Guid>
            {
                RoleId = role.Id,
                ClaimType = CustomClaimTypes.Permissions,
                ClaimValue = claim
            });

            await _context.RoleClaims.AddRangeAsync(claimsToAdd);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Ok();
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();
            return Result.Fail(exception.Message);
        }
    }

    public async Task<Result> DeleteRoleAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role is null)
            return Result.Fail(new RoleNotFoundError(roleName));

        var identityResult = await _roleManager.DeleteAsync(role);

        if (!identityResult.Succeeded)
            return Result.Fail(identityResult.Errors.Select(error => error.Description));

        return Result.Ok();
    }

    public async Task<Result> UpdateRoleNameAsync(string oldRoleName, string newRoleName)
    {
        var role = await _roleManager.FindByNameAsync(oldRoleName);
        if (role is null)
            return Result.Fail(new RoleNotFoundError(oldRoleName));

        if (await _roleManager.RoleExistsAsync(newRoleName))
            return Result.Fail(new RoleExistError(newRoleName));

        await _roleManager.SetRoleNameAsync(role, newRoleName);

        var identityResult = await _roleManager.UpdateAsync(role);

        if (!identityResult.Succeeded)
            return Result.Fail(identityResult.Errors.Select(error => error.Description));

        return Result.Ok();
    }

    private static List<string> NormalizeClaims(List<string> claims)
    {
        return claims
            .Where(claim => !string.IsNullOrWhiteSpace(claim))
            .Select(claim => claim.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
