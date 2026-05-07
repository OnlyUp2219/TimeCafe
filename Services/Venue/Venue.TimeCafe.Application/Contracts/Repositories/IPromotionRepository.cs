namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(Guid promotionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetActiveByDateAsync(DateTimeOffset date, CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    Task<Promotion> CreateAsync(Promotion promotion, CancellationToken cancellationToken = default);
    Task<Promotion> UpdateAsync(Promotion promotion, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid promotionId, CancellationToken cancellationToken = default);
    Task<bool> ActivateAsync(Guid promotionId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid promotionId, CancellationToken cancellationToken = default);
}

