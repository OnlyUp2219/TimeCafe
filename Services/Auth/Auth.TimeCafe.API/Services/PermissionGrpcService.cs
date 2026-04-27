namespace Auth.TimeCafe.API.Services;

public class PermissionGrpcService(ApplicationDbContext dbContext) 
    : BuildingBlocks.Permissions.Grpc.PermissionGrpcService.PermissionGrpcServiceBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public override async Task<GetUserPermissionsResponse> GetUserPermissions(
        GetUserPermissionsRequest request, 
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid UserId format"));
        }

        var permissions = await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_dbContext.Set<IdentityRoleClaim<Guid>>(),
                ur => ur.RoleId,
                rc => rc.RoleId,
                (ur, rc) => rc)
            .Where(rc => rc.ClaimType == CustomClaimTypes.Permissions)
            .Select(rc => rc.ClaimValue)
            .Where(v => v != null)
            .Distinct()
            .ToListAsync(context.CancellationToken);

        var response = new GetUserPermissionsResponse();
        response.Permissions.AddRange(permissions!);
        
        return response;
    }
}
