namespace Auth.TimeCafe.Infrastructure.Services;

public class RbacRepository : IRbacRepository
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly HybridCache _cache;

    public RbacRepository(
        ApplicationDbContext context,
        RoleManager<IdentityRole<Guid>> roleManager,
        HybridCache cache)
    {
        _context = context;
        _roleManager = roleManager;
        _cache = cache;
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
        var uniqueClaims = claims == null ? new List<string>() : NormalizeClaims(claims);

        var useTransaction = !string.Equals(_context.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal);
        await using var transaction = useTransaction ? await _context.Database.BeginTransactionAsync() : null;
        try
        {
            var role = new IdentityRole<Guid>(roleName);
            var identityResult = await _roleManager.CreateAsync(role);

            if (!identityResult.Succeeded)
            {
                if (transaction is not null)
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
            if (transaction is not null)
                await transaction.CommitAsync();
            await InvalidatePermissionsCacheAsync();

            return Result.Ok();
        }
        catch (Exception exception)
        {
            if (transaction is not null)
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

        var useTransaction = !string.Equals(_context.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal);
        await using var transaction = useTransaction ? await _context.Database.BeginTransactionAsync() : null;
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
            if (transaction is not null)
                await transaction.CommitAsync();
            await InvalidatePermissionsCacheByRoleAsync(roleName);

            return Result.Ok();
        }
        catch (Exception exception)
        {
            if (transaction is not null)
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

        await InvalidatePermissionsCacheByRoleAsync(roleName);

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

        await InvalidatePermissionsCacheByRoleAsync(oldRoleName);
        await InvalidatePermissionsCacheByRoleAsync(newRoleName);

        return Result.Ok();
    }

    public async Task<Result> AssignRoleToUserAsync(Guid userId, string roleName)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
            return Result.Fail(new UserNotFoundError(userId));

        var role = await _roleManager.FindByNameAsync(roleName);
        if (role is null)
            return Result.Fail(new RoleNotFoundError(roleName));

        var alreadyAssigned = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

        if (alreadyAssigned)
            return Result.Fail(new UserAlreadyInRoleError(userId, roleName));

        _context.UserRoles.Add(new IdentityUserRole<Guid>
        {
            UserId = userId,
            RoleId = role.Id
        });

        await _context.SaveChangesAsync();
        await InvalidatePermissionsCacheByUserAsync(userId);
        await InvalidatePermissionsCacheByRoleAsync(roleName);

        return Result.Ok();
    }

    public async Task<Result> RemoveRoleFromUserAsync(Guid userId, string roleName)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
            return Result.Fail(new UserNotFoundError(userId));

        var role = await _roleManager.FindByNameAsync(roleName);
        if (role is null)
            return Result.Fail(new RoleNotFoundError(roleName));

        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

        if (userRole is null)
            return Result.Fail(new UserRoleNotAssignedError(userId, roleName));

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();

        await InvalidatePermissionsCacheByUserAsync(userId);
        await InvalidatePermissionsCacheByRoleAsync(roleName);

        return Result.Ok();
    }

    private async Task InvalidatePermissionsCacheAsync(CancellationToken ct = default)
    {
        await _cache.RemoveByTagAsync(PermissionClaimsCacheKeys.PermissionsTag, ct);
    }

    private async Task InvalidatePermissionsCacheByRoleAsync(string roleName, CancellationToken ct = default)
    {
        await _cache.RemoveByTagAsync(PermissionClaimsCacheKeys.RolePermissionsTag(roleName), ct);
    }

    private async Task InvalidatePermissionsCacheByUserAsync(Guid userId, CancellationToken ct = default)
    {
        await _cache.RemoveByTagAsync(PermissionClaimsCacheKeys.UserPermissionsTag(userId), ct);
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
