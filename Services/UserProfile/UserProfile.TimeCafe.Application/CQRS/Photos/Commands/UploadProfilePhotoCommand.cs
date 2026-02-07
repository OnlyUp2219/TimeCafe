using UserProfile.TimeCafe.Domain.DTOs;

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
    public static UploadProfilePhotoResult Ok(string key, string url, long size, string contentType) =>
        new(true, Key: key, Url: url, Size: size, ContentType: contentType,
            StatusCode: 201, Message: "Фото загружено");
    public static UploadProfilePhotoResult Failed() =>
        new(false, Code: "UploadFailed", Message: "Не удалось загрузить фото", StatusCode: 500);
    public static UploadProfilePhotoResult ProfileNotFound() =>
        new(false, Code: "ProfileNotFound", Message: "Профиль не найден", StatusCode: 404);
}

public class UploadProfilePhotoCommandValidator : AbstractValidator<UploadProfilePhotoCommand>
{
    public UploadProfilePhotoCommandValidator(IOptions<PhotoOptions> photoOptions)
    {
        var opts = photoOptions.Value;
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Такого пользователя не существует")
           .NotNull().WithMessage("Такого пользователя не существует")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Такого пользователя не существует");

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
    IPhotoModerationService moderationService,
    ILogger<UploadProfilePhotoCommandHandler> logger) : IRequestHandler<UploadProfilePhotoCommand, UploadProfilePhotoResult>
{
    private readonly IProfilePhotoStorage _storage = storage;
    private readonly IUserRepositories _userRepository = userRepository;
    private readonly IPhotoModerationService _moderationService = moderationService;
    private readonly ILogger<UploadProfilePhotoCommandHandler> _logger = logger;

    public async Task<UploadProfilePhotoResult> Handle(UploadProfilePhotoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = Guid.Parse(request.UserId);
            var profile = await _userRepository.GetProfileByIdAsync(userId, cancellationToken);
            if (profile is null)
                return UploadProfilePhotoResult.ProfileNotFound();

            var uploadStream = new MemoryStream();
            try
            {
                await request.Data.CopyToAsync(uploadStream, cancellationToken);
                uploadStream.Position = 0;

                var moderationBytes = uploadStream.ToArray();
                using var moderationStream = new MemoryStream(moderationBytes);
                var moderationResult = await _moderationService.ModeratePhotoAsync(moderationStream, cancellationToken);

                if (!moderationResult.IsSafe)
                {
                    _logger.LogWarning(
                        "Фото отклонено модерацией для UserId={UserId}. Причина: {Reason}. Scores: {Scores}",
                        request.UserId,
                        moderationResult.Reason,
                        moderationResult.Scores != null ? string.Join(", ", moderationResult.Scores.Select(s => $"{s.Key}={s.Value:F2}")) : "N/A");

                    return new UploadProfilePhotoResult(
                        false,
                        Code: "PhotoRejected",
                        Message: $"Фото отклонено: {moderationResult.Reason}",
                        StatusCode: 400);
                }

                _logger.LogInformation(
                    "Фото прошло модерацию для UserId={UserId}. Scores: {Scores}",
                    request.UserId,
                    moderationResult.Scores != null ? string.Join(", ", moderationResult.Scores.Select(s => $"{s.Key}={s.Value:F2}")) : "N/A");

                uploadStream.Position = 0;

                var result = await _storage.UploadAsync(userId, uploadStream, request.ContentType, request.FileName, cancellationToken);

                if (!result.Success || result.Key is null || result.Url is null || result.Size is null || result.ContentType is null)
                    return UploadProfilePhotoResult.Failed();

                profile.PhotoUrl = result.Url;
                await _userRepository.UpdateProfileAsync(profile, cancellationToken);

                return UploadProfilePhotoResult.Ok(result.Key, result.Url, result.Size.Value, result.ContentType);
            }
            finally
            {
                uploadStream.Dispose();
            }
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(UploadProfilePhotoResult.Failed(), ex);
        }
    }
}