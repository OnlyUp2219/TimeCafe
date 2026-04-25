namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(Guid promotionId, CancellationToken ct = default);
    Task<IEnumerable<Promotion>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Promotion>> GetActiveAsync(CancellationToken ct = default);
    Task<IEnumerable<Promotion>> GetActiveByDateAsync(DateTimeOffset date, CancellationToken ct = default);

    Task<Promotion> CreateAsync(Promotion promotion, CancellationToken ct = default);
    Task<Promotion> UpdateAsync(Promotion promotion, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid promotionId, CancellationToken ct = default);
    Task<bool> ActivateAsync(Guid promotionId, CancellationToken ct = default);
    Task<bool> DeactivateAsync(Guid promotionId, CancellationToken ct = default);
}

