namespace Venue.TimeCafe.Domain.Contracts.Repositories;

public interface ITariffRepository
{
    Task<Tariff?> GetByIdAsync(int tariffId);
    Task<IEnumerable<Tariff>> GetAllAsync();
    Task<IEnumerable<Tariff>> GetActiveAsync();
    Task<IEnumerable<Tariff>> GetByBillingTypeAsync(BillingType billingType);
    Task<IEnumerable<Tariff>> GetPagedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalCountAsync();

    Task<Tariff> CreateAsync(Tariff tariff);
    Task<Tariff> UpdateAsync(Tariff tariff);
    Task<bool> DeleteAsync(int tariffId);
    Task<bool> ActivateAsync(int tariffId);
    Task<bool> DeactivateAsync(int tariffId);
}
