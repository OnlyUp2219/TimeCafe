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

public sealed class UpdateRoleNameCommandHandler(
    IRbacRepository rbacRepository,
    IPublisher publisher) : ICommandHandler<UpdateRoleNameCommand>
{
    public async Task<Result> Handle(UpdateRoleNameCommand request, CancellationToken cancellationToken = default)
    {
        if (Roles.IsSystemRole(request.OldRoleName))
            return Result.Fail(new SystemRoleModificationError(request.OldRoleName));

        if (Roles.IsSystemRole(request.NewRoleName))
            return Result.Fail(new SystemRoleModificationError(request.NewRoleName));

        var result = await rbacRepository.UpdateRoleNameAsync(request.OldRoleName, request.NewRoleName);
        if (result.IsSuccess)
        {
            await publisher.Publish(new Events.RoleClaimsChangedEvent(request.OldRoleName), cancellationToken);
            await publisher.Publish(new Events.RoleClaimsChangedEvent(request.NewRoleName), cancellationToken);
        }
        return result;
    }
}
