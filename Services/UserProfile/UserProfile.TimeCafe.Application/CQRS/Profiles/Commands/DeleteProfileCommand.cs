namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;

public record DeleteProfileCommand(Guid UserId) : ICommand;

public class DeleteProfileCommandValidator : AbstractValidator<DeleteProfileCommand>
{
    public DeleteProfileCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Такого пользователя не существует");

    }
}

public class DeleteProfileCommandHandler(IUserRepositories userRepositories) : ICommandHandler<DeleteProfileCommand>
{
    private readonly IUserRepositories _userRepositories = userRepositories;

    public async Task<Result> Handle(DeleteProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _userRepositories.GetProfileByIdAsync(request.UserId, cancellationToken);
            if (existing == null)
                return Result.Fail(new ProfileNotFoundError());

            await _userRepositories.DeleteProfileAsync(request.UserId, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
