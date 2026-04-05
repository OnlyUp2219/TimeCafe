namespace Auth.TimeCafe.Application.CQRS.RBAC.Query;

public sealed class GetPermissionsQuery() : IQuery<List<string>>;

public sealed class GetPermissionsQueryHandler(IRbacRepository rbacRepository) : IQueryHandler<GetPermissionsQuery, List<string>>
{
    private readonly IRbacRepository _rbacRepository = rbacRepository;

    public Task<Result<List<string>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = _rbacRepository.GetPermissions();

        if (permissions is null || permissions.Count == 0)
            return Task.FromResult(Result.Fail<List<string>>(new PermissionsNotFoundError()));

        return Task.FromResult(Result.Ok(permissions));
    }
}
