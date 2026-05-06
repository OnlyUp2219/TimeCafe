namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;

public record DeleteProfileCommand(Guid UserId) : ICommand;

public class DeleteProfileCommandValidator : AbstractValidator<DeleteProfileCommand>
{
    public DeleteProfileCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Такого пользователя не существует");
    }
}

public class DeleteProfileCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<DeleteProfileCommand>
{
    public async Task<Result> Handle(DeleteProfileCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await uow.Profiles.GetByIdAsync(request.UserId, cancellationToken);
            if (existing == null)
                return Result.Fail(new ProfileNotFoundError());

            await uow.Profiles.DeleteAsync(request.UserId, cancellationToken);
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
