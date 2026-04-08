namespace Auth.TimeCafe.Application.CQRS.RBAC.Query;

public sealed record GetRoleByNameQuery(string RoleName) : IQuery<RolesResponse>;

public sealed class GetRoleByNameQueryValidator : AbstractValidator<GetRoleByNameQuery>
{
	public GetRoleByNameQueryValidator()
	{
		RuleFor(x => x.RoleName)
			.NotEmpty().WithMessage("Роль не найдена");
	}
}

public sealed class GetRoleByNameQueryHandler(IRbacRepository rbacRepository) : IQueryHandler<GetRoleByNameQuery, RolesResponse>
{
	private readonly IRbacRepository _rbacRepository = rbacRepository;

	public async Task<Result<RolesResponse>> Handle(GetRoleByNameQuery request, CancellationToken cancellationToken)
	{
		var roles = await _rbacRepository.GetRolesAsync();

		if (roles is null || roles.Count == 0)
			return Result.Fail<RolesResponse>(new RolesNotFoundError());

		var role = roles.FirstOrDefault(r =>
			string.Equals(r.RoleName, request.RoleName, StringComparison.OrdinalIgnoreCase));

		if (role is null)
			return Result.Fail<RolesResponse>(new RoleNotFoundError(request.RoleName));

		return Result.Ok(role);
	}
}
