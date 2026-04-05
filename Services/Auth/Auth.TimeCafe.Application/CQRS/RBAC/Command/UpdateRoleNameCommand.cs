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
    private readonly IRbacRepository _rbacRepository = rbacRepository;

    public async Task<Result> Handle(UpdateRoleNameCommand request, CancellationToken cancellationToken)
    {
        if (request.OldRoleName == Roles.Admin && request.OldRoleName == Roles.Client)
            return Result.Fail(new SystemRoleModificationError(request.OldRoleName));

        return await _rbacRepository.UpdateRoleNameAsync(request.OldRoleName, request.NewRoleName);
    }
}
