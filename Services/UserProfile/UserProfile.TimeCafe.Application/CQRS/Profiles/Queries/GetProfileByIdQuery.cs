namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetProfileByIdQuery(Guid Id) : IQuery<Profile>;

public class GetProfileByIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetProfileByIdQuery, Profile>
{
    public async Task<Result<Profile>> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await uow.Profiles.GetByIdAsync(request.Id, cancellationToken);
            return profile != null 
                ? Result.Ok(ProfilePhotoUrlMapper.WithApiUrl(profile)) 
                : Result.Fail(new ProfileNotFoundError());
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
