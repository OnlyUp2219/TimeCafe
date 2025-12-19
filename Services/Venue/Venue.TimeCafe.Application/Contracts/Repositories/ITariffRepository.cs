namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface ITariffRepository
{
    Task<TariffWithThemeDto?> GetByIdAsync(Guid tariffId);
    Task<IEnumerable<TariffWithThemeDto>> GetAllAsync();
    Task<IEnumerable<TariffWithThemeDto>> GetActiveAsync();
    Task<IEnumerable<TariffWithThemeDto>> GetByBillingTypeAsync(BillingType billingType);
    Task<IEnumerable<TariffWithThemeDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalCountAsync();

    Task<Tariff> CreateAsync(Tariff tariff);
    Task<Tariff> UpdateAsync(Tariff tariff);
    Task<bool> DeleteAsync(Guid tariffId);
    Task<bool> ActivateAsync(Guid tariffId);
    Task<bool> DeactivateAsync(Guid tariffId);
}
