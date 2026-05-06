using BuildingBlocks.Contracts;
using UserProfile.TimeCafe.Domain.Models;

namespace UserProfile.TimeCafe.Domain.Contracts;

public interface IUserRepositories : IRepository<Profile, Guid>
{
    Task<IEnumerable<Profile?>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Profile?>> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalPageAsync(CancellationToken cancellationToken = default);
    Task CreateEmptyAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Profile>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}
