namespace UserProfile.TimeCafe.Application.CQRS.Photos.Commands;

public record DeleteProfilePhotoCommand(Guid UserId) : ICommand;

public class DeleteProfilePhotoCommandValidator : AbstractValidator<DeleteProfilePhotoCommand>
{
    public DeleteProfilePhotoCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Такого пользователя не существует");
    }
}

public class DeleteProfilePhotoCommandHandler(IProfilePhotoStorage storage, IUnitOfWork uow, IPublisher publisher) : ICommandHandler<DeleteProfilePhotoCommand>
{
    public async Task<Result> Handle(DeleteProfilePhotoCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await uow.Profiles.GetByIdAsync(request.UserId, cancellationToken);
            if (profile is null)
                return Result.Fail(new ProfileNotFoundError());

            var deleted = await storage.DeleteAsync(request.UserId, cancellationToken);
            if (!deleted)
                return Result.Fail(new PhotoNotFoundError());

            profile.PhotoUrl = null;
            await uow.Profiles.UpdateAsync(profile, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
            await publisher.Publish(new ProfileChangedEvent(profile.UserId), cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
