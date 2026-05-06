namespace UserProfile.TimeCafe.Application.CQRS.Photos.Commands;

public record UploadProfilePhotoCommand(Guid UserId, Stream Data, string ContentType, string FileName, long Size) : ICommand<string>;

public class UploadProfilePhotoCommandValidator : AbstractValidator<UploadProfilePhotoCommand>
{
    public UploadProfilePhotoCommandValidator(IOptions<PhotoOptions> photoOptions)
    {
        var opts = photoOptions.Value;
        RuleFor(x => x.UserId).ValidGuidEntityId("Такого пользователя не существует");
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
    IUnitOfWork uow,
    IPublisher publisher,
    IPhotoModerationService moderationService,
    ILogger<UploadProfilePhotoCommandHandler> logger) : ICommandHandler<UploadProfilePhotoCommand, string>
{
    public async Task<Result<string>> Handle(UploadProfilePhotoCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await uow.Profiles.GetByIdAsync(request.UserId, cancellationToken);
            if (profile is null)
                return Result.Fail(new ProfileNotFoundError());

            var uploadStream = new MemoryStream();
            try
            {
                await request.Data.CopyToAsync(uploadStream, cancellationToken);
                uploadStream.Position = 0;

                var moderationBytes = uploadStream.ToArray();
                await using var moderationStream = new MemoryStream(moderationBytes);
                var moderationResult = await moderationService.ModeratePhotoAsync(moderationStream, cancellationToken);

                if (!moderationResult.IsSafe)
                {
                    logger.LogWarning(
                        "Фото отклонено модерацией для UserId={UserId}. Причина: {Reason}. Scores: {Scores}",
                        request.UserId,
                        moderationResult.Reason,
                        moderationResult.Scores != null ? string.Join(", ", moderationResult.Scores.Select(s => $"{s.Key}={s.Value:F2}")) : "N/A");

                    return Result.Fail(new Error($"Фото отклонено: {moderationResult.Reason}").WithMetadata("ErrorCode", "400").WithMetadata("Code", "PhotoRejected"));
                }

                logger.LogInformation(
                    "Фото прошло модерацию для UserId={UserId}. Scores: {Scores}",
                    request.UserId,
                    moderationResult.Scores != null ? string.Join(", ", moderationResult.Scores.Select(s => $"{s.Key}={s.Value:F2}")) : "N/A");

                uploadStream.Position = 0;

                var result = await storage.UploadAsync(request.UserId, uploadStream, request.ContentType, request.FileName, cancellationToken);

                if (!result.Success || result.Key is null || result.Size is null || result.ContentType is null)
                    return Result.Fail(new FailedError());

                profile.PhotoUrl = result.Key;
                await uow.Profiles.UpdateAsync(profile, cancellationToken);
                await uow.SaveChangesAsync(cancellationToken);
                await publisher.Publish(new ProfileChangedEvent(profile.UserId), cancellationToken);

                var responseUrl = ProfilePhotoUrlMapper.BuildApiUrl(request.UserId);
                return Result.Ok(responseUrl);
            }
            finally
            {
                await uploadStream.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
