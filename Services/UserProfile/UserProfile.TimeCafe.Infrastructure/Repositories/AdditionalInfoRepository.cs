namespace UserProfile.TimeCafe.Infrastructure.Repositories;

public class AdditionalInfoRepository(
    ApplicationDbContext context,
    IDistributedCache cache,
    ILogger<AdditionalInfoRepository> cacheLogger) : IAdditionalInfoRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger _cacheLogger = cacheLogger;

    public async Task<IEnumerable<AdditionalInfo>> GetAdditionalInfosByUserIdAsync(
        string userId,
        CancellationToken? cancellationToken = null)
    {
        var cacheKey = CacheKeys.AdditionalInfo_ByUserId(userId);
        var cached = await CacheHelper.GetAsync<IEnumerable<AdditionalInfo>>(
            _cache,
            _cacheLogger,
            cacheKey).ConfigureAwait(false);

        if (cached != null)
            return cached;

        var infos = await _context.AdditionalInfos
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken ?? CancellationToken.None)
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            cacheKey,
            infos).ConfigureAwait(false);

        return infos;
    }

    public async Task<AdditionalInfo?> GetAdditionalInfoByIdAsync(
        int infoId,
        CancellationToken? cancellationToken = null)
    {
        var cacheKey = CacheKeys.AdditionalInfo_ById(infoId);
        var cached = await CacheHelper.GetAsync<AdditionalInfo>(
            _cache,
            _cacheLogger,
            cacheKey).ConfigureAwait(false);

        if (cached != null)
            return cached;

        var info = await _context.AdditionalInfos
            .FirstOrDefaultAsync(i => i.InfoId == infoId, cancellationToken ?? CancellationToken.None)
            .ConfigureAwait(false);

        if (info != null)
        {
            await CacheHelper.SetAsync(
                _cache,
                _cacheLogger,
                cacheKey,
                info).ConfigureAwait(false);
        }

        return info;
    }

    public async Task<AdditionalInfo> CreateAdditionalInfoAsync(
        AdditionalInfo info,
        CancellationToken? cancellationToken = null)
    {
        info.CreatedAt = DateTime.UtcNow;
        _context.AdditionalInfos.Add(info);
        await _context.SaveChangesAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.AdditionalInfo_All,
            CacheKeys.AdditionalInfo_ByUserId(info.UserId)).ConfigureAwait(false);
        return info;
    }

    public async Task<AdditionalInfo?> UpdateAdditionalInfoAsync(
        AdditionalInfo info,
        CancellationToken? cancellationToken = null)
    {
        var existingInfo = await _context.AdditionalInfos.FindAsync(info.InfoId);

        if (existingInfo == null)
        {
            return null;
        }

        _context.Entry(existingInfo).CurrentValues.SetValues(info);
        await _context.SaveChangesAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.AdditionalInfo_All,
            CacheKeys.AdditionalInfo_ById(info.InfoId),
            CacheKeys.AdditionalInfo_ByUserId(info.UserId)).ConfigureAwait(false);
        return info;
    }

    public async Task<bool> DeleteAdditionalInfoAsync(
        int infoId,
        CancellationToken? cancellationToken = null)
    {
        var info = await _context.AdditionalInfos.FindAsync(infoId);

        if (info == null)
        {
            return false;
        }

        _context.AdditionalInfos.Remove(info);
        await _context.SaveChangesAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.AdditionalInfo_All,
            CacheKeys.AdditionalInfo_ById(infoId),
            CacheKeys.AdditionalInfo_ByUserId(info.UserId)).ConfigureAwait(false);
        return true;
    }
}
