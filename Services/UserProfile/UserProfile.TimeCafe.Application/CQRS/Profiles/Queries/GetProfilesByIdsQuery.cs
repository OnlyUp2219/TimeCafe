namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetProfilesByIdsQuery(IEnumerable<Guid> Ids) : IQuery<IEnumerable<Profile>>;

public class GetProfilesByIdsQueryHandler(IUnitOfWork uow) : IQueryHandler<GetProfilesByIdsQuery, IEnumerable<Profile>>
{
    public async Task<Result<IEnumerable<Profile>>> Handle(GetProfilesByIdsQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var profiles = await uow.Profiles.GetByIdsAsync(request.Ids, cancellationToken);
            return Result.Ok<IEnumerable<Profile>>(ProfilePhotoUrlMapper.WithApiUrl(profiles));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
