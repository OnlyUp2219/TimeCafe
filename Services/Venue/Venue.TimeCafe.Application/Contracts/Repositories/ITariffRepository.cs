namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface ITariffRepository
{
    Task<TariffWithThemeDto?> GetByIdAsync(Guid tariffId, CancellationToken ct = default);
    Task<IEnumerable<TariffWithThemeDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<TariffWithThemeDto>> GetActiveAsync(CancellationToken ct = default);
    Task<IEnumerable<TariffWithThemeDto>> GetByBillingTypeAsync(BillingType billingType, CancellationToken ct = default);
    Task<IEnumerable<TariffWithThemeDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);

    Task<Tariff> CreateAsync(Tariff tariff, CancellationToken ct = default);
    Task<Tariff> UpdateAsync(Tariff tariff, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid tariffId, CancellationToken ct = default);
    Task<bool> ActivateAsync(Guid tariffId, CancellationToken ct = default);
    Task<bool> DeactivateAsync(Guid tariffId, CancellationToken ct = default);
}

