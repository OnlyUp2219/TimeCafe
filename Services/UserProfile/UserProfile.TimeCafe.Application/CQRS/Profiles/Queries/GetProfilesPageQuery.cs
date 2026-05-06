namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetProfilesPageQuery(int PageNumber, int PageSize) : IQuery<IEnumerable<Profile>>;

public class GetProfilesPageQueryHandler(IUnitOfWork uow) : IQueryHandler<GetProfilesPageQuery, IEnumerable<Profile>>
{
    public async Task<Result<IEnumerable<Profile>>> Handle(GetProfilesPageQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var profiles = await uow.Profiles.GetPageAsync(request.PageNumber, request.PageSize, cancellationToken);
            return Result.Ok<IEnumerable<Profile>>(ProfilePhotoUrlMapper.WithApiUrl(profiles.Where(p => p != null)!));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
