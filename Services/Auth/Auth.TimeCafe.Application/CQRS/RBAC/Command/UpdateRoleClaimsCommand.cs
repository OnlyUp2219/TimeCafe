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
    private readonly IRbacRepository _rbacRepository = rbacRepository;

    public async Task<Result> Handle(UpdateRoleClaimsCommand request, CancellationToken cancellationToken)
    {
        if (request.RoleName == Roles.Admin && request.RoleName == Roles.Client)
            return Result.Fail(new SystemRoleModificationError(request.RoleName));

        return await _rbacRepository.UpdateRoleClaimsAsync(request.RoleName, request.Claims);
    }
}
