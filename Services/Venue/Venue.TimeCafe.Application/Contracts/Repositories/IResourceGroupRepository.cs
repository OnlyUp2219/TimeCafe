namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IResourceGroupRepository : IRepository<ResourceGroup, Guid>
{
    Task<IEnumerable<ResourceGroup>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ResourceGroup>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
