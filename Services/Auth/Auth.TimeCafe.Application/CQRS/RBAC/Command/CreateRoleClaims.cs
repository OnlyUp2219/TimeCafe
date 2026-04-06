namespace Auth.TimeCafe.Application.CQRS.RBAC.Command;

public sealed record CreateRoleClaimsCommand(string RoleName, List<string> Claims) : ICommand;

public sealed class CreateRoleClaimsValidator : AbstractValidator<CreateRoleClaimsCommand>
{
    public CreateRoleClaimsValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty();
        RuleFor(x => x.Claims).NotEmpty();
    }
}

public sealed class CreateRoleClaimsCommandHandler(IRbacRepository rbacRepository) : ICommandHandler<CreateRoleClaimsCommand>
{
    public async Task<Result> Handle(CreateRoleClaimsCommand request, CancellationToken cancellationToken)
    {
        if (await rbacRepository.RoleExistsAsync(request.RoleName))
            return Result.Fail(new RoleExistError(request.RoleName));

        return await rbacRepository.CreateRoleClaimsAsync(request.RoleName, request.Claims);
    }
}
