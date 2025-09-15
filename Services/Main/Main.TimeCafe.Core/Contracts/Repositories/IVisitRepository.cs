using TimeCafe.Core.Models;

namespace TimeCafe.Core.Contracts.Repositories;

public interface IVisitRepository
{
    // Queries
    Task<IEnumerable<Visit>> GetActiveVisitsAsync();
    Task<Visit> GetActiveVisitByClientAsync(int clientId);
    Task<bool> IsClientActiveAsync(int clientId);
    Task<bool> IsClientAlreadyEnteredAsync(int clientId);

    // Commands
    Task<decimal> CalculateVisitCostAsync(Visit visit);
    TimeSpan GetVisitDuration(Visit visit);
    Task<Visit> EnterClientAsync(int clientId, int tariffId, int minimumEntryMinutes);
    Task<Visit> ExitClientAsync(int visitId);

    // TODO: Автоматический выход всех посетителей при закрытии заведения
    // Task ExitAllVisitorsAsync(string reason = "Автоматический выход при закрытии заведения");
}