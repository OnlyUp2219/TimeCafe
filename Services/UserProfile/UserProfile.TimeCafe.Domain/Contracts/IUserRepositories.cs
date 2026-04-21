namespace UserProfile.TimeCafe.Domain.Contracts;

public interface IUserRepositories
{
    Task<IEnumerable<Profile?>> GetAllProfilesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Profile?>> GetProfilesPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalPageAsync(CancellationToken cancellationToken = default);
    Task<Profile?> GetProfileByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Profile?> CreateProfileAsync(Profile profile, CancellationToken cancellationToken = default);
    Task<Profile?> UpdateProfileAsync(Profile profile, CancellationToken cancellationToken = default);
    Task DeleteProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CreateEmptyAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Profile>> GetProfilesByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}
