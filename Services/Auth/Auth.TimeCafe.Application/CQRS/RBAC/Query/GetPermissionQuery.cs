namespace Auth.TimeCafe.Application.CQRS.RBAC.Query;

public sealed record GetPermissionQuery(string Permission) : IQuery<string>;

public sealed class GetPermissionQueryValidator : AbstractValidator<GetPermissionQuery>
{
	public GetPermissionQueryValidator()
	{
		RuleFor(x => x.Permission)
			.NotEmpty().WithMessage("Разрешение не найдено");
	}
}

public sealed class GetPermissionQueryHandler(IRbacRepository rbacRepository) : IQueryHandler<GetPermissionQuery, string>
{
	private readonly IRbacRepository _rbacRepository = rbacRepository;

	public Task<Result<string>> Handle(GetPermissionQuery request, CancellationToken cancellationToken)
	{
		var permissions = _rbacRepository.GetPermissions();

		if (permissions is null || permissions.Count == 0)
			return Task.FromResult(Result.Fail<string>(new PermissionsNotFoundError()));

		var permission = permissions.FirstOrDefault(p =>
			string.Equals(p, request.Permission, StringComparison.OrdinalIgnoreCase));

		if (string.IsNullOrWhiteSpace(permission))
			return Task.FromResult(Result.Fail<string>(new PermissionNotFoundError(request.Permission)));

		return Task.FromResult(Result.Ok(permission));
	}
}
