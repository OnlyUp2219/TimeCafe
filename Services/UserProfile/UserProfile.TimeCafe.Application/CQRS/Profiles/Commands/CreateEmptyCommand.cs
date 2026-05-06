namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;

public record CreateEmptyCommand(Guid UserId) : ICommand;

public class CreateEmptyCommandValidator : AbstractValidator<CreateEmptyCommand>
{
    public CreateEmptyCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Такого пользователя не существует");
    }
}

public class CreateEmptyCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<CreateEmptyCommand>
{
    public async Task<Result> Handle(CreateEmptyCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await uow.Profiles.GetByIdAsync(request.UserId, cancellationToken);
            if (existing != null)
                return Result.Fail(new ProfileAlreadyExistsError());

            await uow.Profiles.CreateEmptyAsync(request.UserId, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
            await publisher.Publish(new ProfileChangedEvent(request.UserId), cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
