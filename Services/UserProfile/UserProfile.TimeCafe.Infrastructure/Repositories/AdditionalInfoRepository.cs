namespace UserProfile.TimeCafe.Infrastructure.Repositories;

public class AdditionalInfoRepository(
    ApplicationDbContext context,
    HybridCache cache) : IAdditionalInfoRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<IEnumerable<AdditionalInfo>> GetAdditionalInfosByUserIdAsync(
        Guid userId,
        CancellationToken? cancellationToken = null)
    {
        var ct = cancellationToken ?? CancellationToken.None;
        return await _cache.GetOrCreateAsync(
            CacheKeys.AdditionalInfo_ByUserId(userId),
            async token => await _context.AdditionalInfos
                .AsNoTracking()
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(token),
            tags: [CacheTags.AdditionalInfos, CacheTags.AdditionalInfoByUser(userId)],
            cancellationToken: ct);
    }

    public async Task<AdditionalInfo?> GetAdditionalInfoByIdAsync(
        Guid infoId,
        CancellationToken? cancellationToken = null)
    {
        var ct = cancellationToken ?? CancellationToken.None;
        return await _cache.GetOrCreateAsync(
            CacheKeys.AdditionalInfo_ById(infoId),
            async token => await _context.AdditionalInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InfoId == infoId, token),
            tags: [CacheTags.AdditionalInfos],
            cancellationToken: ct);
    }

    public async Task<AdditionalInfo> CreateAdditionalInfoAsync(
        AdditionalInfo info,
        CancellationToken? cancellationToken = null)
    {
        var ct = cancellationToken ?? CancellationToken.None;
        info.CreatedAt = DateTimeOffset.UtcNow;
        _context.AdditionalInfos.Add(info);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.AdditionalInfos, ct);
        await _cache.RemoveByTagAsync(CacheTags.AdditionalInfoByUser(info.UserId), ct);

        return info;
    }

    public async Task<AdditionalInfo?> UpdateAdditionalInfoAsync(
        AdditionalInfo info,
        CancellationToken? cancellationToken = null)
    {
        var ct = cancellationToken ?? CancellationToken.None;
        var existingInfo = await _context.AdditionalInfos.FindAsync(info.InfoId);

        if (existingInfo == null)
            return null;

        _context.Entry(existingInfo).CurrentValues.SetValues(info);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.AdditionalInfos, ct);
        await _cache.RemoveByTagAsync(CacheTags.AdditionalInfoByUser(info.UserId), ct);

        return info;
    }

    public async Task<bool> DeleteAdditionalInfoAsync(
        Guid infoId,
        CancellationToken? cancellationToken = null)
    {
        var ct = cancellationToken ?? CancellationToken.None;
        var info = await _context.AdditionalInfos.FindAsync(infoId);

        if (info == null)
            return false;

        _context.AdditionalInfos.Remove(info);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.AdditionalInfos, ct);
        await _cache.RemoveByTagAsync(CacheTags.AdditionalInfoByUser(info.UserId), ct);

        return true;
    }
}
