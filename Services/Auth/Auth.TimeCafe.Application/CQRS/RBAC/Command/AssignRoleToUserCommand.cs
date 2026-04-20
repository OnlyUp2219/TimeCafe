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

public sealed class AssignRoleToUserCommandHandler(IRbacRepository rbacRepository) : ICommandHandler<AssignRoleToUserCommand>
{
    public async Task<Result> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        if (request.RoleName.Equals(BuildingBlocks.Permissions.Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase))
            return Result.Fail(new SuperAdminModificationError());

        return await rbacRepository.AssignRoleToUserAsync(request.UserId, request.RoleName);
    }
}
