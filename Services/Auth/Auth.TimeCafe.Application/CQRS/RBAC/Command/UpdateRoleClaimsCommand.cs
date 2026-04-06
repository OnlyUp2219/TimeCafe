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
        if (IsSystemRole(request.RoleName))
            return Result.Fail(new SystemRoleModificationError(request.RoleName));

        return await rbacRepository.UpdateRoleClaimsAsync(request.RoleName, request.Claims);
    }

    private static bool IsSystemRole(string roleName)
    {
        return string.Equals(roleName, Roles.Admin, StringComparison.OrdinalIgnoreCase)
               || string.Equals(roleName, Roles.Client, StringComparison.OrdinalIgnoreCase);
    }
}
