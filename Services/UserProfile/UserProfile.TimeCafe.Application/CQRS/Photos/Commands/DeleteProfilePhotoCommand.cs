namespace UserProfile.TimeCafe.Application.CQRS.Photos.Commands;

public record DeleteProfilePhotoCommand(Guid UserId) : ICommand;

public class DeleteProfilePhotoCommandValidator : AbstractValidator<DeleteProfilePhotoCommand>
{
    public DeleteProfilePhotoCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Такого пользователя не существует");
    }
}

public class DeleteProfilePhotoCommandHandler(IProfilePhotoStorage storage, IUserRepositories userRepository) : ICommandHandler<DeleteProfilePhotoCommand>
{
    private readonly IProfilePhotoStorage _storage = storage;
    private readonly IUserRepositories _userRepository = userRepository;

    public async Task<Result> Handle(DeleteProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _userRepository.GetProfileByIdAsync(request.UserId, cancellationToken);
            if (profile is null)
                return Result.Fail(new ProfileNotFoundError());

            var deleted = await _storage.DeleteAsync(request.UserId, cancellationToken);
            if (!deleted)
                return Result.Fail(new PhotoNotFoundError());

            profile.PhotoUrl = null;
            await _userRepository.UpdateProfileAsync(profile, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
