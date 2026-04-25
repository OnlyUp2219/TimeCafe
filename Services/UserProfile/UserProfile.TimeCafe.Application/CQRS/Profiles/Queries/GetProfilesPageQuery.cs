namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetProfilesPageQuery(int PageNumber, int PageSize) : IQuery<GetProfilesPageResponse>;

public record GetProfilesPageResponse(IEnumerable<Profile> Profiles, int PageNumber, int PageSize, int TotalCount);
public class GetProfilesPageQueryValidator : AbstractValidator<GetProfilesPageQuery>
{
    public GetProfilesPageQueryValidator()
    {
        RuleFor(x => x.PageNumber).ValidPageNumber();

        RuleFor(x => x.PageSize).ValidPageSize();
    }
}

public class GetProfilesPageQueryHandler(IUserRepositories repositories) : IQueryHandler<GetProfilesPageQuery, GetProfilesPageResponse>
{
    private readonly IUserRepositories _repositories = repositories;

    public async Task<Result<GetProfilesPageResponse>> Handle(GetProfilesPageQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var profiles = await _repositories.GetProfilesPageAsync(request.PageNumber, request.PageSize, cancellationToken);
            var totalCount = await _repositories.GetTotalPageAsync(cancellationToken);
            var nonNullProfiles = profiles.Where(p => p != null).Cast<Profile>();

            var responseProfiles = ProfilePhotoUrlMapper.WithApiUrl(nonNullProfiles);
            return Result.Ok(new GetProfilesPageResponse(responseProfiles, request.PageNumber, request.PageSize, totalCount));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
