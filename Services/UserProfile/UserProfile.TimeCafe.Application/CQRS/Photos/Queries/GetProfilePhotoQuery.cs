namespace UserProfile.TimeCafe.Application.CQRS.Photos.Queries;

public record GetProfilePhotoQuery(Guid UserId) : IQuery<PhotoStreamDto>;

public class GetProfilePhotoQueryValidator : AbstractValidator<GetProfilePhotoQuery>
{
    public GetProfilePhotoQueryValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Такого пользователя не существует");
    }
}

public class GetProfilePhotoQueryHandler(IProfilePhotoStorage storage) : IQueryHandler<GetProfilePhotoQuery, PhotoStreamDto>
{
    private readonly IProfilePhotoStorage _storage = storage;
    public async Task<Result<PhotoStreamDto>> Handle(GetProfilePhotoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _storage.GetAsync(request.UserId, cancellationToken);
            if (data is null)
                return Result.Fail(new PhotoNotFoundError());

            return Result.Ok(data);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
