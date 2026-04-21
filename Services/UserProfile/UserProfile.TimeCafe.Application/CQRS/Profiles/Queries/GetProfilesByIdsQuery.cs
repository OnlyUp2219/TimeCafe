using BuildingBlocks.Contracts.CQRS;
using UserProfile.TimeCafe.Application.Helpers;

namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetProfilesByIdsQuery(IEnumerable<Guid> UserIds) : IQuery<List<Profile>>;

public class GetProfilesByIdsQueryHandler(IUserRepositories repository) : IQueryHandler<GetProfilesByIdsQuery, List<Profile>>
{
    private readonly IUserRepositories _repository = repository;

    public async Task<Result<List<Profile>>> Handle(GetProfilesByIdsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var profiles = await _repository.GetProfilesByIdsAsync(request.UserIds, cancellationToken);
            
            var mappedProfiles = profiles
                .Select(ProfilePhotoUrlMapper.WithApiUrl)
                .ToList();

            return Result.Ok(mappedProfiles);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Не удалось получить профили").CausedBy(ex));
        }
    }
}
