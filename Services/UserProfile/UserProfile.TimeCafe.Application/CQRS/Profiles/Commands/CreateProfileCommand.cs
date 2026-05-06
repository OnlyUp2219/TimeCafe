namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;

public record CreateProfileCommand(Guid UserId, string FirstName, string LastName, Gender Gender) : ICommand<Profile>;

public class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Такого пользователя не существует");
        RuleFor(x => x.FirstName).ValidName("Имя");
        RuleFor(x => x.LastName).ValidName("Фамилия");
    }
}

public class CreateProfileCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<CreateProfileCommand, Profile>
{
    public async Task<Result<Profile>> Handle(CreateProfileCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await uow.Profiles.GetByIdAsync(request.UserId, cancellationToken);
            if (existing != null)
                return Result.Fail(new ProfileAlreadyExistsError());

            var profile = new Profile
            {
                UserId = request.UserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Gender = request.Gender,
                ProfileStatus = ProfileStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var created = await uow.Profiles.CreateAsync(profile, cancellationToken);

            if (created == null)
                return Result.Fail(new CreateFailedError());

            await uow.SaveChangesAsync(cancellationToken);
            await publisher.Publish(new ProfileChangedEvent(created.UserId), cancellationToken);

            return Result.Ok(created);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
