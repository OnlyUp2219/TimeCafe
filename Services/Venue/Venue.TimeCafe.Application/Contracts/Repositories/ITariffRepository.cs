namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface ITariffRepository : IRepository<Tariff, Guid>
{
    Task<TariffWithThemeDto?> GetWithThemeByIdAsync(Guid tariffId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TariffWithThemeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TariffWithThemeDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TariffWithThemeDto>> GetByBillingTypeAsync(BillingType billingType, CancellationToken cancellationToken = default);
    Task<IEnumerable<TariffWithThemeDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    Task<bool> ActivateAsync(Guid tariffId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid tariffId, CancellationToken cancellationToken = default);
}

