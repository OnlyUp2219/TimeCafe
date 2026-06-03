namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IResourceRepository : IRepository<Resource, Guid>
{
    Task<IEnumerable<Resource>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Resource>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<bool> AnyWithGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
}
