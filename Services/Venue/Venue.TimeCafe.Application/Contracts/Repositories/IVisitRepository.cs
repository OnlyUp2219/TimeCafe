namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IVisitRepository : IRepository<Visit, Guid>
{
    Task<VisitWithTariffDto?> GetWithTariffByIdAsync(Guid visitId, CancellationToken cancellationToken = default);
    Task<VisitWithTariffDto?> GetActiveVisitByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<VisitWithTariffDto>> GetActiveVisitsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<VisitWithTariffDto>> GetVisitHistoryByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<VisitWithTariffDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<bool> HasActiveVisitAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<VisitWithTariffDto>> GetPendingVisitsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> IsResourceBusyAsync(Guid resourceId, CancellationToken cancellationToken = default);
    Task<bool> IsResourceBusyAsync(Guid resourceId, Guid? excludeVisitId, CancellationToken cancellationToken = default);
    Task<bool> AnyWithTariffIdAsync(Guid tariffId, CancellationToken cancellationToken = default);
}

