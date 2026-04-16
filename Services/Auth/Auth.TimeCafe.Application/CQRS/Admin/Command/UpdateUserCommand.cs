namespace Auth.TimeCafe.Application.CQRS.Admin.Command;

public sealed record UpdateUserCommand(
    Guid UserId,
    string? Email,
    string? UserName,
    bool? EmailConfirmed,
    bool? LockoutEnabled,
    DateTimeOffset? LockoutEnd) : IRequest<Result>;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email is not null);
        RuleFor(x => x.UserName).MinimumLength(3).MaximumLength(256).When(x => x.UserName is not null);
    }
}

public sealed class UpdateUserCommandHandler(
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            return Result.Fail(new Error("Пользователь не найден").WithMetadata("StatusCode", 404));

        if (request.Email is not null)
        {
            user.Email = request.Email;
            user.NormalizedEmail = request.Email.ToUpperInvariant();
        }
        if (request.UserName is not null)
        {
            user.UserName = request.UserName;
            user.NormalizedUserName = request.UserName.ToUpperInvariant();
        }
        if (request.EmailConfirmed.HasValue)
            user.EmailConfirmed = request.EmailConfirmed.Value;
        if (request.LockoutEnabled.HasValue)
            user.LockoutEnabled = request.LockoutEnabled.Value;
        if (request.LockoutEnd.HasValue)
            user.LockoutEnd = request.LockoutEnd.Value;

        var identityResult = await userManager.UpdateAsync(user);
        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(e => new Error(e.Description)).ToList();
            return Result.Fail(errors);
        }

        return Result.Ok();
    }
}
