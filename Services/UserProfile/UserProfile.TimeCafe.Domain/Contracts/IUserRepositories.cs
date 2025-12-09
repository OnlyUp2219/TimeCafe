namespace UserProfile.TimeCafe.Domain.Contracts;

public interface IUserRepositories
{
    Task<IEnumerable<Profile?>> GetAllProfilesAsync(CancellationToken? cancellationToken);
    Task<IEnumerable<Profile?>> GetProfilesPageAsync(int pageNumber, int pageSize, CancellationToken? cancellationToken);
    Task<int> GetTotalPageAsync(CancellationToken? cancellationToken);
    Task<Profile?> GetProfileByIdAsync(Guid userId, CancellationToken? cancellationToken);
    Task<Profile?> CreateProfileAsync(Profile profile, CancellationToken? cancellationToken);
    Task<Profile?> UpdateProfileAsync(Profile profile, CancellationToken? cancellationToken);
    Task DeleteProfileAsync(Guid userId, CancellationToken? cancellationToken);
    Task CreateEmptyAsync(Guid userId, CancellationToken? cancellationToken);

}
