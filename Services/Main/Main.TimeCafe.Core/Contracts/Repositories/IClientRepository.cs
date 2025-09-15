using TimeCafe.Core.Models;

namespace TimeCafe.Core.Contracts.Repositories;

public interface IClientRepository
{
    // Queries
    Task<IEnumerable<Client>> GetAllClientsAsync();
    Task<Client?> GetClientByIdAsync(int clientId);
    Task<IEnumerable<Client>> GetClientsPageAsync(int pageNumber, int pageSize);
    Task<IEnumerable<ClientStatus>> GetClientStatusesAsync();
    Task<IEnumerable<Gender>> GetGendersAsync();
    Task<bool> IsPhoneConfirmedAsync(int clientId);
    Task<int> GetTotalPageAsync();

    // Commands
    Task<Client> CreateClientAsync(Client client);
    Task<Client> UpdateClientAsync(Client client);
    Task<bool> DeleteClientAsync(int clientId);
    Task<bool> UpdateClientStatusAsync(int clientId, int statusId);
    Task<bool> SetClientActiveAsync(int clientId);
    Task<bool> SetClientDraftAsync(int clientId);
    Task<bool> SetClientRejectedAsync(int clientId, string reason);
}