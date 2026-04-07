using Microsoft.Extensions.Caching.Hybrid;
using Npgsql;

namespace BuildingBlocks.Permissions;

public sealed class PermissionClaimsEnrichmentTransformer(
    HybridCache cache,
    IConfiguration configuration,
    ILogger<PermissionClaimsEnrichmentTransformer> logger)
    : Microsoft.AspNetCore.Authentication.IClaimsTransformation
{
    private readonly HybridCache _cache = cache;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<PermissionClaimsEnrichmentTransformer> _logger = logger;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated is not true)
            return principal;

        if (principal.HasClaim(claim => claim.Type == CustomClaimTypes.Permissions))
            return principal;

        var subject = principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("nameid");

        if (!Guid.TryParse(subject, out var userId))
            return principal;

        var authConnectionString = ResolveAuthConnectionString();
        if (string.IsNullOrWhiteSpace(authConnectionString))
            return principal;

        var roleTags = principal
            .FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(PermissionClaimsCacheKeys.RolePermissionsTag)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var cacheTags = new List<string>
        {
            PermissionClaimsCacheKeys.PermissionsTag,
            PermissionClaimsCacheKeys.UserPermissionsTag(userId)
        };

        cacheTags.AddRange(roleTags);

        IReadOnlyCollection<string> permissions;

        try
        {
            permissions = await _cache.GetOrCreateAsync(
                PermissionClaimsCacheKeys.UserPermissionsKey(userId),
                async token => await LoadPermissionsAsync(authConnectionString, userId, token),
                options: new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromMinutes(5)
                },
                tags: cacheTags);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enrich permissions for user {UserId}", userId);
            return principal;
        }

        if (permissions.Count == 0)
            return principal;

        if (principal.Identity is not ClaimsIdentity identity)
            return principal;

        foreach (var permission in permissions)
        {
            if (!identity.HasClaim(CustomClaimTypes.Permissions, permission))
                identity.AddClaim(new Claim(CustomClaimTypes.Permissions, permission));
        }

        return principal;
    }

    private string? ResolveAuthConnectionString()
    {
        var explicitAuthConnection = _configuration.GetConnectionString("AuthConnection")
            ?? _configuration["ConnectionStrings:AuthConnection"];

        if (!string.IsNullOrWhiteSpace(explicitAuthConnection))
            return explicitAuthConnection;

        var defaultConnection = _configuration.GetConnectionString("DefaultConnection")
            ?? _configuration["ConnectionStrings:DefaultConnection"];

        if (string.IsNullOrWhiteSpace(defaultConnection))
            return null;

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(defaultConnection)
            {
                Database = "Auth.TimeCafe"
            };

            return builder.ConnectionString;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<IReadOnlyCollection<string>> LoadPermissionsAsync(
        string connectionString,
        Guid userId,
        CancellationToken ct)
    {
        const string sql = @"
    SELECT DISTINCT rc.""ClaimValue""
    FROM ""AspNetUserRoles"" ur
    INNER JOIN ""AspNetRoleClaims"" rc ON rc.""RoleId"" = ur.""RoleId""
    WHERE ur.""UserId"" = @userId
      AND rc.""ClaimType"" = @claimType
      AND rc.""ClaimValue"" IS NOT NULL";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("claimType", CustomClaimTypes.Permissions);

        var permissions = new HashSet<string>(StringComparer.Ordinal);

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var value = reader.GetString(0);
            if (!string.IsNullOrWhiteSpace(value))
                permissions.Add(value);
        }

        return permissions.ToList();
    }
}
