namespace Auth.TimeCafe.Application.CQRS.RBAC.Command;

public sealed record UpdateRoleClaimsCommand(string RoleName, List<string> Claims) : ICommand;

public sealed class UpdateRoleClaimsCommandValidator : AbstractValidator<UpdateRoleClaimsCommand>
{
    public UpdateRoleClaimsCommandValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty();
        RuleFor(x => x.Claims).NotEmpty();
    }
}

public sealed class UpdateRoleClaimsCommandHandler(IRbacRepository rbacRepository) : ICommandHandler<UpdateRoleClaimsCommand>
{
    public async Task<Result> Handle(UpdateRoleClaimsCommand request, CancellationToken cancellationToken)
    {
        if (Roles.IsSystemRole(request.RoleName))
            return Result.Fail(new SystemRoleModificationError(request.RoleName));

        return await rbacRepository.UpdateRoleClaimsAsync(request.RoleName, request.Claims);
    }
}
