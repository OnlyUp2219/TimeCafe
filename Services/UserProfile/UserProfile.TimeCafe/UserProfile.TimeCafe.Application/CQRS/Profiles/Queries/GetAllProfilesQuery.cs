namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record class GetAllProfilesQuery() : IRequest<IEnumerable<Profile?>>;

public class GetAllProfilesQueryValidator : AbstractValidator<GetAllProfilesQuery>
{
    public GetAllProfilesQueryValidator()
    {

    }
}
public class GetAllProfilesQueryHandler(IUserRepositories userRepositories) : IRequestHandler<GetAllProfilesQuery, IEnumerable<Profile?>>
{
    private readonly IUserRepositories _userRepositories = userRepositories;
    public async Task<IEnumerable<Profile?>> Handle(GetAllProfilesQuery request, CancellationToken cancellationToken)
    {
        return await _userRepositories.GetAllProfilesAsync(cancellationToken).ConfigureAwait(false);
    }
}