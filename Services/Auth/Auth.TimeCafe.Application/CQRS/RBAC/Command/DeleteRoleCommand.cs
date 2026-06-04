namespace Auth.TimeCafe.Application.CQRS.RBAC.Command;

public sealed record DeleteRoleCommand(string RoleName) : ICommand;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty();
    }
}

public sealed class DeleteRoleCommandHandler(
    IRbacRepository rbacRepository,
    IPublisher publisher) : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken = default)
    {
        if (Roles.IsSystemRole(request.RoleName))
            return Result.Fail(new SystemRoleModificationError(request.RoleName));

        var result = await rbacRepository.DeleteRoleAsync(request.RoleName);
        if (result.IsSuccess)
        {
            await publisher.Publish(new Events.RoleClaimsChangedEvent(request.RoleName), cancellationToken);
        }
        return result;
    }
}
