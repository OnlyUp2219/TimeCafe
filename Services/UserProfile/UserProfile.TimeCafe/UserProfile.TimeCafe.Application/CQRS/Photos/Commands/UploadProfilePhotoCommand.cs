namespace UserProfile.TimeCafe.Application.CQRS.Photos.Commands;

public record UploadProfilePhotoCommand(string UserId, Stream Data, string ContentType, string FileName, long Size) : IRequest<UploadProfilePhotoResult>;

public record UploadProfilePhotoResult(
    bool Success,
    string? Code = null,
    List<ErrorItem>? Errors = null,
    string? Message = null,
    int? StatusCode = null,
    string? Key = null,
    string? Url = null,
    long? Size = null,
    string? ContentType = null) : ICqrsResultV2
{
    public static UploadProfilePhotoResult Ok(string key, string url, long size, string contentType) => new(true, Key: key, Url: url, Size: size, ContentType: contentType, StatusCode: 201, Message: "Фото загружено");
    public static UploadProfilePhotoResult Failed() => new(false, Code: "UploadFailed", Message: "Не удалось загрузить фото", StatusCode: 500);
    public static UploadProfilePhotoResult ProfileNotFound() => new(false, Code: "ProfileNotFound", Message: "Профиль не найден", StatusCode: 404);
}

public class UploadProfilePhotoCommandValidator : AbstractValidator<UploadProfilePhotoCommand>
{
    public UploadProfilePhotoCommandValidator(IOptions<PhotoOptions> photoOptions)
    {
        var opts = photoOptions.Value;
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(450);
        RuleFor(x => x.ContentType)
            .Must(ct => opts.AllowedContentTypes.Contains(ct))
            .WithMessage($"Неподдерживаемый тип файла. Допустимые: {string.Join(", ", opts.AllowedContentTypes)}");
        RuleFor(x => x.Size)
            .GreaterThan(0)
            .LessThanOrEqualTo(opts.MaxSizeBytes)
            .WithMessage($"Размер файла превышает лимит {opts.MaxSizeBytes / (1024 * 1024)}MB");
    }
}

public class UploadProfilePhotoCommandHandler(
    IProfilePhotoStorage storage, 
    IUserRepositories userRepository,
    IPhotoModerationService moderationService) : IRequestHandler<UploadProfilePhotoCommand, UploadProfilePhotoResult>
{
    private readonly IProfilePhotoStorage _storage = storage;
    private readonly IUserRepositories _userRepository = userRepository;
    private readonly IPhotoModerationService _moderationService = moderationService;

    public async Task<UploadProfilePhotoResult> Handle(UploadProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _userRepository.GetProfileByIdAsync(request.UserId, cancellationToken);
            if (profile is null)
                return UploadProfilePhotoResult.ProfileNotFound();

            // Модерация фото перед загрузкой
            using var moderationStream = new MemoryStream();
            await request.Data.CopyToAsync(moderationStream, cancellationToken);
            moderationStream.Position = 0;
            
            var moderationResult = await _moderationService.ModeratePhotoAsync(moderationStream, cancellationToken);
            
            if (!moderationResult.IsSafe)
            {
                return new UploadProfilePhotoResult(
                    false, 
                    Code: "PhotoRejected", 
                    Message: $"Фото отклонено: {moderationResult.Reason}", 
                    StatusCode: 400);
            }

            // Сбрасываем позицию после модерации
            moderationStream.Position = 0;

            var result = await _storage.UploadAsync(request.UserId, moderationStream, request.ContentType, request.FileName, cancellationToken);
            if (!result.Success || result.Key is null || result.Url is null || result.Size is null || result.ContentType is null)
                return UploadProfilePhotoResult.Failed();

            profile.PhotoUrl = result.Url;
            await _userRepository.UpdateProfileAsync(profile, cancellationToken);

            return UploadProfilePhotoResult.Ok(result.Key, result.Url, result.Size.Value, result.ContentType);
        }
        catch (Exception)
        {
            return UploadProfilePhotoResult.Failed();
        }
    }
}