namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;
//TODO: rewrite dto
public record UpdateProfileCommand(Profile User) : ICommand<Profile>;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.User).NotNull().WithMessage("Профиль обязателен");
        RuleFor(x => x.User.UserId).ValidGuidId("Такого пользователя не существует");
        RuleFor(x => x.User.FirstName).ValidName("Имя");
        RuleFor(x => x.User.LastName).ValidName("Фамилия");
        RuleFor(x => x.User.Gender).IsInEnum().WithMessage("Пол указан некорректно");
        RuleFor(x => x.User.BirthDate).ValidBirthDate();
    }
}

public class UpdateProfileCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<UpdateProfileCommand, Profile>
{
    public async Task<Result<Profile>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await uow.Profiles.GetByIdAsync(request.User.UserId, cancellationToken);
            if (existing == null)
                return Result.Fail(new ProfileNotFoundError());

            request.User.PhotoUrl = existing.PhotoUrl;

            if (existing.ProfileStatus == ProfileStatus.Banned)
            {
                request.User.ProfileStatus = ProfileStatus.Banned;
                request.User.BanReason = existing.BanReason;
            }
            else
            {
                var isCompleted =
                    !string.IsNullOrWhiteSpace(request.User.FirstName)
                    && !string.IsNullOrWhiteSpace(request.User.LastName);

                request.User.ProfileStatus = isCompleted ? ProfileStatus.Completed : ProfileStatus.Pending;
            }

            var updated = await uow.Profiles.UpdateAsync(request.User, cancellationToken);

            if (updated == null)
                return Result.Fail(new UpdateFailedError());

            await uow.SaveChangesAsync(cancellationToken);
            await publisher.Publish(new ProfileChangedEvent(updated.UserId), cancellationToken);

            var responseProfile = ProfilePhotoUrlMapper.WithApiUrl(updated);
            return Result.Ok(responseProfile);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
