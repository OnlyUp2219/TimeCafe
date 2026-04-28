namespace BuildingBlocks.Permissions;

public sealed class PermissionClaimsEnrichmentTransformer(
    HybridCache cache,
    PermissionGrpcService.PermissionGrpcServiceClient grpcClient,
    ILogger<PermissionClaimsEnrichmentTransformer> logger)
    : IClaimsTransformation
{
    private readonly HybridCache _cache = cache;
    private readonly PermissionGrpcService.PermissionGrpcServiceClient _grpcClient = grpcClient;
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
                async token => await LoadPermissionsViaGrpcAsync(userId, token),
                options: new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(5),
                    LocalCacheExpiration = TimeSpan.Zero
                },
                tags: cacheTags);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enrich permissions via gRPC for user {UserId}", userId);
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

    private async Task<IReadOnlyCollection<string>> LoadPermissionsViaGrpcAsync(
        Guid userId,
        CancellationToken ct)
    {
        var request = new GetUserPermissionsRequest { UserId = userId.ToString() };
        var response = await _grpcClient.GetUserPermissionsAsync(request, cancellationToken: ct);
        
        return response.Permissions.ToList();
    }
}
