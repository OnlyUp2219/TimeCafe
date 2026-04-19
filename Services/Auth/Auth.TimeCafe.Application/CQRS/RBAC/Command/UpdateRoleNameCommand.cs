namespace Auth.TimeCafe.Application.CQRS.RBAC.Command;

public sealed record UpdateRoleNameCommand(string OldRoleName, string NewRoleName) : ICommand;

public sealed class UpdateRoleNameCommandValidator : AbstractValidator<UpdateRoleNameCommand>
{
    public UpdateRoleNameCommandValidator()
    {
        RuleFor(x => x.OldRoleName).NotEmpty();
        RuleFor(x => x.NewRoleName).NotEmpty();
    }
}

public sealed class UpdateRoleNameCommandHandler(IRbacRepository rbacRepository) : ICommandHandler<UpdateRoleNameCommand>
{
    public async Task<Result> Handle(UpdateRoleNameCommand request, CancellationToken cancellationToken)
    {
        if (Roles.IsSystemRole(request.OldRoleName))
            return Result.Fail(new SystemRoleModificationError(request.OldRoleName));

        if (Roles.IsSystemRole(request.NewRoleName))
            return Result.Fail(new SystemRoleModificationError(request.NewRoleName));

        return await rbacRepository.UpdateRoleNameAsync(request.OldRoleName, request.NewRoleName);
    }
}
