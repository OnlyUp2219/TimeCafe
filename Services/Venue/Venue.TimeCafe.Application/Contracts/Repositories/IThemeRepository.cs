namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IThemeRepository : IRepository<Theme, Guid>
{
    Task<IEnumerable<Theme>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Theme>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}

