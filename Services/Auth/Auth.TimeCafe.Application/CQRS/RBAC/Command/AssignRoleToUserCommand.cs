namespace Auth.TimeCafe.Application.CQRS.RBAC.Command;

public sealed record AssignRoleToUserCommand(Guid UserId, string RoleName) : ICommand;

public sealed class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleName).NotEmpty();
    }
}

public sealed class AssignRoleToUserCommandHandler(
    IRbacRepository rbacRepository,
    IPublisher publisher) : ICommandHandler<AssignRoleToUserCommand>
{
    public async Task<Result> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken = default)
    {
        if (request.RoleName.Equals(BuildingBlocks.Permissions.Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase))
            return Result.Fail(new SuperAdminModificationError());

        var result = await rbacRepository.AssignRoleToUserAsync(request.UserId, request.RoleName);
        if (result.IsSuccess)
        {
            await publisher.Publish(new Events.UserRolesChangedEvent(request.UserId, request.RoleName), cancellationToken);
        }
        return result;
    }
}
