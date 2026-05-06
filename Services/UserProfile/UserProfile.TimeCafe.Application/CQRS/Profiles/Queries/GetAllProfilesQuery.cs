namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetAllProfilesQuery() : IQuery<IEnumerable<Profile>>;

public class GetAllProfilesQueryHandler(IUnitOfWork uow) : IQueryHandler<GetAllProfilesQuery, IEnumerable<Profile>>
{
    public async Task<Result<IEnumerable<Profile>>> Handle(GetAllProfilesQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var profiles = await uow.Profiles.GetAllAsync(cancellationToken);
            return Result.Ok<IEnumerable<Profile>>(ProfilePhotoUrlMapper.WithApiUrl(profiles.Where(p => p != null)!));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
