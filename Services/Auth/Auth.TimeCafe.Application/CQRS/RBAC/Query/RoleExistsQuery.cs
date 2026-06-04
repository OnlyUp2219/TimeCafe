namespace Auth.TimeCafe.Application.CQRS.RBAC.Query;

public sealed record RoleExistsQuery(string RoleName) : IQuery<bool>;


public sealed class RoleExistsQueryHandler(IRbacRepository rbacRepository) : IQueryHandler<RoleExistsQuery, bool>
{
    public async Task<Result<bool>> Handle(RoleExistsQuery request, CancellationToken cancellationToken = default)
    {
        var exists = await rbacRepository.RoleExistsAsync(request.RoleName);
        return Result.Ok(exists);
    }
}
