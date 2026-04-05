namespace Auth.TimeCafe.Application.CQRS.RBAC.Query;

public sealed record GetRoleClaimsByNameQuery(string RoleName) : IQuery<RoleClaimsResponse>;

public sealed class GetRoleClaimsByNameQueryHandler(IRbacRepository rbacRepository)
	: IQueryHandler<GetRoleClaimsByNameQuery, RoleClaimsResponse>
{
	private readonly IRbacRepository _rbacRepository = rbacRepository;

	public async Task<Result<RoleClaimsResponse>> Handle(GetRoleClaimsByNameQuery request, CancellationToken cancellationToken)
	{
		var roleClaims = await _rbacRepository.GetRoleClaimsAsync();

		if (roleClaims is null || roleClaims.Count == 0)
			return Result.Fail<RoleClaimsResponse>(new RoleClaimsNotFoundError());

		var roleClaim = roleClaims.FirstOrDefault(r =>
			string.Equals(r.RoleName, request.RoleName, StringComparison.OrdinalIgnoreCase));

		if (roleClaim is null)
			return Result.Fail<RoleClaimsResponse>(new RoleNotFoundError(request.RoleName));

		return Result.Ok(roleClaim);
	}
}
