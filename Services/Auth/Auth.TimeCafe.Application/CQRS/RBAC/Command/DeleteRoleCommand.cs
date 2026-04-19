namespace Auth.TimeCafe.Application.CQRS.RBAC.Command;

public sealed record DeleteRoleCommand(string RoleName) : ICommand;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty();
    }
}

public sealed class DeleteRoleCommandHandler(IRbacRepository rbacRepository) : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        if (Roles.IsSystemRole(request.RoleName))
            return Result.Fail(new SystemRoleModificationError(request.RoleName));

        return await rbacRepository.DeleteRoleAsync(request.RoleName);
    }
}
