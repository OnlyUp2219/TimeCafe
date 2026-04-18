namespace Auth.TimeCafe.Application.CQRS.Admin.Command;

public sealed record DeleteUserCommand(Guid UserId) : IRequest<Result>;

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
    }
}

public sealed class DeleteUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IUserContext userContext)
    : IRequestHandler<DeleteUserCommand, Result>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == userContext.UserId)
            return Result.Fail(new Error("Нельзя удалить собственный аккаунт").WithMetadata("StatusCode", 403));

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            return Result.Fail(new Error("Пользователь не найден").WithMetadata("StatusCode", 404));

        var identityResult = await userManager.DeleteAsync(user);
        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(e => new Error(e.Description)).ToList();
            return Result.Fail(errors);
        }

        return Result.Ok();
    }
}
