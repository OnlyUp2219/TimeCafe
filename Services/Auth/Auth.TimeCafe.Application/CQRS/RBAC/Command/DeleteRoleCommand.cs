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
    public readonly IRbacRepository _rbacRepository = rbacRepository;

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.RoleName == Roles.Admin && request.RoleName == Roles.Client)
            return Result.Fail(new SystemRoleModificationError(request.RoleName));

        return await _rbacRepository.DeleteRoleAsync(request.RoleName);
    }
}
