namespace Auth.TimeCafe.Application.CQRS.RBAC.Command;

public sealed record RemoveRoleFromUserCommand(Guid UserId, string RoleName) : ICommand;

public sealed class RemoveRoleFromUserCommandValidator : AbstractValidator<RemoveRoleFromUserCommand>
{
    public RemoveRoleFromUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleName).NotEmpty();
    }
}

public sealed class RemoveRoleFromUserCommandHandler(IRbacRepository rbacRepository, IUserContext userContext) : ICommandHandler<RemoveRoleFromUserCommand>
{
    public async Task<Result> Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == userContext.UserId)
            return Result.Fail(new Error("Нельзя снять роль с собственного аккаунта.").WithMetadata("StatusCode", 403));

        return await rbacRepository.RemoveRoleFromUserAsync(request.UserId, request.RoleName);
    }
}
