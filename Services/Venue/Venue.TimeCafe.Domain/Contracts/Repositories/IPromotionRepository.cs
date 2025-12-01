namespace Venue.TimeCafe.Domain.Contracts.Repositories;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(int promotionId);
    Task<IEnumerable<Promotion>> GetAllAsync();
    Task<IEnumerable<Promotion>> GetActiveAsync();
    Task<IEnumerable<Promotion>> GetActiveByDateAsync(DateTime date);

    Task<Promotion> CreateAsync(Promotion promotion);
    Task<Promotion> UpdateAsync(Promotion promotion);
    Task<bool> DeleteAsync(int promotionId);
    Task<bool> ActivateAsync(int promotionId);
    Task<bool> DeactivateAsync(int promotionId);
}
