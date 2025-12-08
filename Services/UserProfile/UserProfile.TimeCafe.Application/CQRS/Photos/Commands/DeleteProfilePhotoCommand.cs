namespace UserProfile.TimeCafe.Application.CQRS.Photos.Commands;

public record DeleteProfilePhotoCommand(Guid UserId) : IRequest<DeleteProfilePhotoResult>;

public record DeleteProfilePhotoResult(bool Success, string? Code = null, string? Message = null, int? StatusCode = null, List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static DeleteProfilePhotoResult PhotoNotFound() => new(false, Code: "PhotoNotFound", Message: "Фото не найдено", StatusCode: 404);
    public static DeleteProfilePhotoResult ProfileNotFound() => new(false, Code: "ProfileNotFound", Message: "Профиль не найден", StatusCode: 404);
    public static DeleteProfilePhotoResult Ok() => new(true, Message: "Фото удалено", StatusCode: 204);
    public static DeleteProfilePhotoResult Failed() => new(false, Code: "PhotoDeleteFailed", Message: "Ошибка удаления фото", StatusCode: 500);
}

public class DeleteProfilePhotoCommandValidator : AbstractValidator<DeleteProfilePhotoCommand>
{
    public DeleteProfilePhotoCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class DeleteProfilePhotoCommandHandler(IProfilePhotoStorage storage, IUserRepositories userRepository) : IRequestHandler<DeleteProfilePhotoCommand, DeleteProfilePhotoResult>
{
    private readonly IProfilePhotoStorage _storage = storage;
    private readonly IUserRepositories _userRepository = userRepository;

    public async Task<DeleteProfilePhotoResult> Handle(DeleteProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _userRepository.GetProfileByIdAsync(request.UserId, cancellationToken);
            if (profile is null)
                return DeleteProfilePhotoResult.ProfileNotFound();

            var deleted = await _storage.DeleteAsync(request.UserId, cancellationToken);
            if (!deleted)
                return DeleteProfilePhotoResult.PhotoNotFound();

            profile.PhotoUrl = null;
            await _userRepository.UpdateProfileAsync(profile, cancellationToken);

            return DeleteProfilePhotoResult.Ok();
        }
        catch (Exception)
        {
            return DeleteProfilePhotoResult.Failed();
        }
    }
}