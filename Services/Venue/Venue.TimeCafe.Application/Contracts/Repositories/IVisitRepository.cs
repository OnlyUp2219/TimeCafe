namespace Venue.TimeCafe.Application.Contracts.Repositories;

public interface IVisitRepository
{
    Task<VisitWithTariffDto?> GetByIdAsync(Guid visitId, CancellationToken ct = default);
    Task<VisitWithTariffDto?> GetActiveVisitByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<VisitWithTariffDto>> GetActiveVisitsAsync(CancellationToken ct = default);
    Task<IEnumerable<VisitWithTariffDto>> GetVisitHistoryByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
    Task<bool> HasActiveVisitAsync(Guid userId, CancellationToken ct = default);

    Task<Visit> CreateAsync(Visit visit, CancellationToken ct = default);
    Task<Visit> UpdateAsync(Visit visit, bool saveChanges = true, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid visitId, CancellationToken ct = default);
}
