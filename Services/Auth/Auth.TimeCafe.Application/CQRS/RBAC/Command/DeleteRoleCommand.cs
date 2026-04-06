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
        if (IsSystemRole(request.RoleName))
            return Result.Fail(new SystemRoleModificationError(request.RoleName));

        return await rbacRepository.DeleteRoleAsync(request.RoleName);
    }

    private static bool IsSystemRole(string roleName)
    {
        return string.Equals(roleName, Roles.Admin, StringComparison.OrdinalIgnoreCase)
               || string.Equals(roleName, Roles.Client, StringComparison.OrdinalIgnoreCase);
    }
}
