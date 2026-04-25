namespace Auth.TimeCafe.Application.CQRS.RBAC.Query;

public sealed record GetRolesQuery() : IQuery<List<RolesResponse>>;

public sealed class GetRolesQueryValidator : AbstractValidator<GetRolesQuery>
{
    public GetRolesQueryValidator()
    {
    }
}

public sealed class GetRolesQueryHandler(IRbacRepository rbacRepository) : IQueryHandler<GetRolesQuery, List<RolesResponse>>
{
    private readonly IRbacRepository _rbacRepository = rbacRepository;

    public async Task<Result<List<RolesResponse>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _rbacRepository.GetRolesAsync();

        if (roles is null || roles.Count == 0)
            return Result.Fail<List<RolesResponse>>(new RolesNotFoundError());

        return Result.Ok(roles);
    }
}
