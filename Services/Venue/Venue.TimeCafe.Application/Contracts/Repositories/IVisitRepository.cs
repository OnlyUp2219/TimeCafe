namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IVisitRepository
{
    Task<VisitWithTariffDto?> GetByIdAsync(Guid visitId, CancellationToken cancellationToken = default);
    Task<VisitWithTariffDto?> GetActiveVisitByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<VisitWithTariffDto>> GetActiveVisitsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<VisitWithTariffDto>> GetVisitHistoryByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<VisitWithTariffDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<bool> HasActiveVisitAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Visit> CreateAsync(Visit visit, CancellationToken cancellationToken = default);
    Task<Visit> UpdateAsync(Visit visit, bool saveChanges = true, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid visitId, CancellationToken cancellationToken = default);
}

