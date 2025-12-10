namespace Venue.TimeCafe.Domain.Contracts.Repositories;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(Guid promotionId);
    Task<IEnumerable<Promotion>> GetAllAsync();
    Task<IEnumerable<Promotion>> GetActiveAsync();
    Task<IEnumerable<Promotion>> GetActiveByDateAsync(DateTime date);

    Task<Promotion> CreateAsync(Promotion promotion);
    Task<Promotion> UpdateAsync(Promotion promotion);
    Task<bool> DeleteAsync(Guid promotionId);
    Task<bool> ActivateAsync(Guid promotionId);
    Task<bool> DeactivateAsync(Guid promotionId);
}
