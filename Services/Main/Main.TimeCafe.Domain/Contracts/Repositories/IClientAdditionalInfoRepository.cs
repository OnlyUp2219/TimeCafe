using Main.TimeCafe.Domain.Models;

namespace Main.TimeCafe.Domain.Contracts.Repositories;

public interface IClientAdditionalInfoRepository
{
    // Queries
    Task<IEnumerable<ClientAdditionalInfo>> GetClientAdditionalInfosAsync(int clientId);
    Task<ClientAdditionalInfo> GetAdditionalInfoByIdAsync(int infoId);

    // Commands
    Task<ClientAdditionalInfo> CreateAdditionalInfoAsync(ClientAdditionalInfo info);
    Task<ClientAdditionalInfo> UpdateAdditionalInfoAsync(ClientAdditionalInfo info);
    Task<bool> DeleteAdditionalInfoAsync(int infoId);
}