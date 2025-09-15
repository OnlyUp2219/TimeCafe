using TimeCafe.Core.Models;

namespace TimeCafe.Core.Contracts.Repositories;

public interface ITariffRepository
{
    // Queries
    Task<IEnumerable<Tariff>> GetAllTariffsAsync();
    Task<IEnumerable<Tariff>> GetTariffsPageAsync(int pageNumber, int pageSize);
    Task<int> GetTotalPageAsync();
    Task<Tariff> GetTariffByIdAsync(int tariffId);

    // Commands
    Task<Tariff> CreateTariffAsync(Tariff tariff);
    Task<Tariff> UpdateTariffAsync(Tariff tariff);
    Task<bool> DeleteTariffAsync(int tariffId);
}