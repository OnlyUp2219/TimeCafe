namespace Auth.TimeCafe.Application.CQRS.RBAC.Query;

public sealed record GetRoleClaimsQuery() : IQuery<List<RoleClaimsResponse>>;

public sealed class GetRoleClaimsQueryHandler(IRbacRepository rbacRepository) : IQueryHandler<GetRoleClaimsQuery, List<RoleClaimsResponse>>
{
	private readonly IRbacRepository _rbacRepository = rbacRepository;

	public async Task<Result<List<RoleClaimsResponse>>> Handle(GetRoleClaimsQuery request, CancellationToken cancellationToken)
	{
		var roleClaims = await _rbacRepository.GetRoleClaimsAsync();

		if (roleClaims is null || roleClaims.Count == 0)
			return Result.Fail<List<RoleClaimsResponse>>(new RoleClaimsNotFoundError());

		return Result.Ok(roleClaims);
	}
}
