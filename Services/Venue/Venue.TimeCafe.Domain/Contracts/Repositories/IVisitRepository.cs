namespace Venue.TimeCafe.Domain.Contracts.Repositories;

public interface IVisitRepository
{
    Task<Visit?> GetByIdAsync(int visitId);
    Task<Visit?> GetActiveVisitByUserAsync(string userId);
    Task<IEnumerable<Visit>> GetActiveVisitsAsync();
    Task<IEnumerable<Visit>> GetVisitHistoryByUserAsync(string userId, int pageNumber = 1, int pageSize = 20);
    Task<bool> HasActiveVisitAsync(string userId);

    Task<Visit> CreateAsync(Visit visit);
    Task<Visit> UpdateAsync(Visit visit);
    Task<bool> DeleteAsync(int visitId);
}