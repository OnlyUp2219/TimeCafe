namespace UserProfile.TimeCafe.Domain.Contracts;

public interface IAdditionalInfoRepository : IRepository<AdditionalInfo, Guid>
{
    Task<IEnumerable<AdditionalInfo>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<AdditionalInfo> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
