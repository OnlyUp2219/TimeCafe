namespace Auth.TimeCafe.Infrastructure.Services;

public class RbacRepository : IRbacRepository
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public RbacRepository(ApplicationDbContext context, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
    }

    public List<string> GetPermissions()
    {
        return _context.RoleClaims
            .Where(p => p.ClaimType == "permissions")
            .Select(p => p.ClaimValue)
            .Distinct()
            .ToList()!;
    }

    public async Task<List<RoleClaimsResponse>> GetRoleClaimsAsync()
    {
        return await _context.Roles
            .Select(role => new RoleClaimsResponse
            {
                RoleId = role.Id,
                RoleName = role.Name!,
                Claims = _context.RoleClaims
                    .Where(rc => rc.RoleId == role.Id)
                    .Select(rc => rc.ClaimValue)
                    .ToList()!
            })
            .ToListAsync();
    }

    public async Task<List<RolesResponse>> GetRolesAsync()
    {
        return await _context.Roles
                    .Select(r => new RolesResponse
                    {
                        RoleId = r.Id,
                        RoleName = r.Name!
                    })
                    .ToListAsync();
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _context.Roles.AnyAsync(r => r.Name == roleName);
    }

    public async Task<Result> CreateRoleClaimsAsync(string RoleName, List<string> Claims)
    {
        var role = new IdentityRole<Guid>(RoleName);
        await _roleManager.CreateAsync(role);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var claimsToAdd = Claims.Select(c => new IdentityRoleClaim<Guid>
            {
                RoleId = role.Id,
                ClaimType = CustomClaimTypes.Permissions,
                ClaimValue = c
            });

            await _context.RoleClaims.AddRangeAsync(claimsToAdd);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> UpdateRoleClaimsAsync(string RoleName, List<string> NewClaims)
    {
        var role = await _roleManager.FindByNameAsync(RoleName);
        if (role == null)
            return Result.Fail(new RoleNotFoundError(RoleName));

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var oldClaims = _context.RoleClaims.Where(rc => rc.RoleId == role.Id);
            _context.RoleClaims.RemoveRange(oldClaims);

            var claimsToAdd = NewClaims.Select(c => new IdentityRoleClaim<Guid>
            {
                RoleId = role.Id,
                ClaimType = CustomClaimTypes.Permissions,
                ClaimValue = c
            }); 

            await _context.RoleClaims.AddRangeAsync(claimsToAdd);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> DeleteRoleAsync(string RoleName)
    {
        var role = await _roleManager.FindByNameAsync(RoleName);
        if (role == null)
            return Result.Fail(new RoleNotFoundError(RoleName));

        var identityResult = await _roleManager.DeleteAsync(role);

        if (!identityResult.Succeeded)
        {
            return Result.Fail(identityResult.Errors.Select(e => e.Description));
        }

        return Result.Ok();
    }

    public async Task<Result> UpdateRoleNameAsync(string OldRoleName,string NewRoleName)
    {
        var role = await _roleManager.FindByNameAsync(OldRoleName);
        if (role == null)
            return Result.Fail(new RoleNotFoundError(OldRoleName));

        await _roleManager.SetRoleNameAsync(role, NewRoleName);

        var identityResult = await _roleManager.UpdateAsync(role);

        if (!identityResult.Succeeded)
        {
            return Result.Fail(identityResult.Errors.Select(e => e.Description));
        }

        return Result.Ok();
    }
}
