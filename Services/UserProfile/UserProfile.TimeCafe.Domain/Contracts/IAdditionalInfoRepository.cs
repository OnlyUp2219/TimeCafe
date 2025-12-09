namespace UserProfile.TimeCafe.Domain.Contracts;

public interface IAdditionalInfoRepository
{
    Task<IEnumerable<AdditionalInfo>> GetAdditionalInfosByUserIdAsync(Guid userId, CancellationToken? cancellationToken = null);
    Task<AdditionalInfo?> GetAdditionalInfoByIdAsync(Guid infoId, CancellationToken? cancellationToken = null);

    Task<AdditionalInfo> CreateAdditionalInfoAsync(AdditionalInfo info, CancellationToken? cancellationToken = null);
    Task<AdditionalInfo?> UpdateAdditionalInfoAsync(AdditionalInfo info, CancellationToken? cancellationToken = null);
    Task<bool> DeleteAdditionalInfoAsync(Guid infoId, CancellationToken? cancellationToken = null);
}
