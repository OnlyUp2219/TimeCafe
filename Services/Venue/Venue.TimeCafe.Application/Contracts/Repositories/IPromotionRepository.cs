namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IPromotionRepository : IRepository<Promotion, Guid>
{
    Task<IEnumerable<Promotion>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetActiveByDateAsync(DateTimeOffset date, CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

