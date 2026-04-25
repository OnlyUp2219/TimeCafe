namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetAllProfilesQuery() : IQuery<IEnumerable<Profile>>;
public class GetAllProfilesQueryValidator : AbstractValidator<GetAllProfilesQuery>
{
    public GetAllProfilesQueryValidator()
    {
        // Нет параметров для валидации
    }
}

public class GetAllProfilesQueryHandler(IUserRepositories userRepositories) : IQueryHandler<GetAllProfilesQuery, IEnumerable<Profile>>
{
    private readonly IUserRepositories _userRepositories = userRepositories;

    public async Task<Result<IEnumerable<Profile>>> Handle(GetAllProfilesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var profiles = await _userRepositories.GetAllProfilesAsync(cancellationToken);
            var nonNullProfiles = profiles.Where(p => p != null).Cast<Profile>();
            var responseProfiles = ProfilePhotoUrlMapper.WithApiUrl(nonNullProfiles);
            return Result.Ok<IEnumerable<Profile>>(responseProfiles);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
