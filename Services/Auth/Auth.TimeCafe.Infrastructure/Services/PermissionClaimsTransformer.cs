using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Hybrid;

namespace Auth.TimeCafe.Infrastructure.Services;

public sealed class PermissionClaimsTransformer(
    ApplicationDbContext context,
    HybridCache cache) : IClaimsTransformation
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            return principal;

        if (principal.HasClaim(claim => claim.Type == CustomClaimTypes.Permissions))
            return principal;

        var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? principal.FindFirstValue("sub");
        if (!Guid.TryParse(subject, out var userId))
            return principal;

        var permissions = await _cache.GetOrCreateAsync(
            PermissionClaimsCacheKeys.UserPermissionsKey(userId),
            token => LoadPermissionsAsync(userId, token),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            },
            tags: [
                PermissionClaimsCacheKeys.AllPermissionsTag,
                PermissionClaimsCacheKeys.UserPermissionsTag(userId)
            ]);

        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim(CustomClaimTypes.Permissions, permission));
        }

        return principal;
    }

    private async ValueTask<List<string>> LoadPermissionsAsync(Guid userId, CancellationToken token)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(
                _context.RoleClaims.Where(rc => rc.ClaimType == CustomClaimTypes.Permissions),
                ur => ur.RoleId,
                rc => rc.RoleId,
                (_, rc) => rc.ClaimValue)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct()
            .ToListAsync(token);
    }
}