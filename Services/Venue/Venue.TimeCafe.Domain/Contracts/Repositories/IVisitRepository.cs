namespace Venue.TimeCafe.Domain.Contracts.Repositories;

public interface IVisitRepository
{
    Task<VisitWithTariffDto?> GetByIdAsync(Guid visitId);
    Task<VisitWithTariffDto?> GetActiveVisitByUserAsync(Guid userId);
    Task<IEnumerable<VisitWithTariffDto>> GetActiveVisitsAsync();
    Task<IEnumerable<VisitWithTariffDto>> GetVisitHistoryByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
    Task<bool> HasActiveVisitAsync(Guid userId);

    Task<Visit> CreateAsync(Visit visit);
    Task<Visit> UpdateAsync(Visit visit);
    Task<bool> DeleteAsync(Guid visitId);
}