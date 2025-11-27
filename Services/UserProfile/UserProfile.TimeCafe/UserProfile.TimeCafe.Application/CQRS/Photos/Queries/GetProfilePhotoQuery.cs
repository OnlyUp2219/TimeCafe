namespace UserProfile.TimeCafe.Application.CQRS.Photos.Queries;

public record GetProfilePhotoQuery(string UserId) : IRequest<GetProfilePhotoResult>;

public record GetProfilePhotoResult(bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Stream? Stream = null,
    string? ContentType = null) : ICqrsResultV2
{
    public static GetProfilePhotoResult NotFound() => new(false, Code: "PhotoNotFound", Message: "Фото не найдено", StatusCode: 404);
    public static GetProfilePhotoResult Ok(Stream stream, string contentType) => new(true, StatusCode: 200,
        Stream: stream, ContentType: contentType);
    public static GetProfilePhotoResult Failed() => new(false, Code: "PhotoGetFailed", Message: "Ошибка получения фото", StatusCode: 500);
}

public class GetProfilePhotoQueryValidator : AbstractValidator<GetProfilePhotoQuery>
{
    public GetProfilePhotoQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(450);
    }
}

public class GetProfilePhotoQueryHandler(IProfilePhotoStorage storage) : IRequestHandler<GetProfilePhotoQuery, GetProfilePhotoResult>
{
    private readonly IProfilePhotoStorage _storage = storage;
    public async Task<GetProfilePhotoResult> Handle(GetProfilePhotoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _storage.GetAsync(request.UserId, cancellationToken);
            if (data is null)
                return GetProfilePhotoResult.NotFound();
            return GetProfilePhotoResult.Ok(data.Stream, data.ContentType);
        }
        catch (Exception)
        {
            return GetProfilePhotoResult.Failed();
        }
    }
}