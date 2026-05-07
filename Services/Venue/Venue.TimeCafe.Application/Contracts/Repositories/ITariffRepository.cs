namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface ITariffRepository
{
    Task<TariffWithThemeDto?> GetByIdAsync(Guid tariffId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TariffWithThemeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TariffWithThemeDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TariffWithThemeDto>> GetByBillingTypeAsync(BillingType billingType, CancellationToken cancellationToken = default);
    Task<IEnumerable<TariffWithThemeDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    Task<Tariff> CreateAsync(Tariff tariff, CancellationToken cancellationToken = default);
    Task<Tariff> UpdateAsync(Tariff tariff, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid tariffId, CancellationToken cancellationToken = default);
    Task<bool> ActivateAsync(Guid tariffId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid tariffId, CancellationToken cancellationToken = default);
}

