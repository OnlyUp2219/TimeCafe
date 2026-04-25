
namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;

public record CreateEmptyCommand(Guid UserId) : ICommand;

public class CreateEmptyCommandValidator : AbstractValidator<CreateEmptyCommand>
{
    public CreateEmptyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .ValidGuidEntityId("Такого пользователя не существует");
    }
}

public class CreateEmptyCommandHandler(IUserRepositories repositories) : ICommandHandler<CreateEmptyCommand>
{
    private readonly IUserRepositories _repositories = repositories;

    public async Task<Result> Handle(CreateEmptyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repositories.GetProfileByIdAsync(request.UserId, cancellationToken);
            if (existing != null)
                return Result.Fail(new ProfileAlreadyExistsError());

            await _repositories.CreateEmptyAsync(request.UserId, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
