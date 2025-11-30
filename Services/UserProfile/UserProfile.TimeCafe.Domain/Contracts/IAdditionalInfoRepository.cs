namespace UserProfile.TimeCafe.Domain.Contracts;

public interface IAdditionalInfoRepository
{
    Task<IEnumerable<AdditionalInfo>> GetAdditionalInfosByUserIdAsync(string userId, CancellationToken? cancellationToken = null);
    Task<AdditionalInfo?> GetAdditionalInfoByIdAsync(int infoId, CancellationToken? cancellationToken = null);

    Task<AdditionalInfo> CreateAdditionalInfoAsync(AdditionalInfo info, CancellationToken? cancellationToken = null);
    Task<AdditionalInfo?> UpdateAdditionalInfoAsync(AdditionalInfo info, CancellationToken? cancellationToken = null);
    Task<bool> DeleteAdditionalInfoAsync(int infoId, CancellationToken? cancellationToken = null);
}
