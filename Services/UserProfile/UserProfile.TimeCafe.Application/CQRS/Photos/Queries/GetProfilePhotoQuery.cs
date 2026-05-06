namespace UserProfile.TimeCafe.Application.CQRS.Photos.Queries;

public record GetProfilePhotoQuery(Guid UserId) : IQuery<PhotoStreamDto>;

public class GetProfilePhotoQueryHandler(IProfilePhotoStorage storage) : IQueryHandler<GetProfilePhotoQuery, PhotoStreamDto>
{
    public async Task<Result<PhotoStreamDto>> Handle(GetProfilePhotoQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await storage.GetAsync(request.UserId, cancellationToken);
            return data != null ? Result.Ok(data) : Result.Fail(new PhotoNotFoundError());
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
