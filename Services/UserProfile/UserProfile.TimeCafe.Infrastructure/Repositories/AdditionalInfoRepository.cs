namespace UserProfile.TimeCafe.Infrastructure.Repositories;

public class AdditionalInfoRepository(ApplicationDbContext context, HybridCache cache) : IAdditionalInfoRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<IEnumerable<AdditionalInfo>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.AdditionalInfo_ByUserId(userId),
            async token => await _context.AdditionalInfos
            .AsNoTracking()
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(token),
        tags: [CacheTags.AdditionalInfos, CacheTags.AdditionalInfoByUser(userId)],
        cancellationToken: cancellationToken);
    }

    public async Task<(IEnumerable<AdditionalInfo> Items, int TotalCount)> GetPagedByUserIdAsync(
    Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.AdditionalInfo_Page(userId, pageNumber, pageSize),
            async token =>
            {
                var query = _context.AdditionalInfos
                    .AsNoTracking()
                    .Where(i => i.UserId == userId);

                var totalCount = await query.CountAsync(token);
                var items = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(token);

                return (Items: (IEnumerable<AdditionalInfo>)items, TotalCount: totalCount);
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.AdditionalInfos, CacheTags.AdditionalInfoByUser(userId)],
            cancellationToken: cancellationToken);
    }

    public async Task<AdditionalInfo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.AdditionalInfo_ById(id),
            async token => await _context.AdditionalInfos
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InfoId == id, token),
            tags: [CacheTags.AdditionalInfos],
            cancellationToken: cancellationToken);
    }

    public async Task<AdditionalInfo> CreateAsync(AdditionalInfo entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        _context.AdditionalInfos.Add(entity);
        return entity;
    }

    public async Task<AdditionalInfo?> UpdateAsync(AdditionalInfo entity, CancellationToken cancellationToken = default)
    {
        var existingInfo = await _context.AdditionalInfos.FindAsync([entity.InfoId], cancellationToken);
        if (existingInfo == null)
            return null;

        _context.Entry(existingInfo).CurrentValues.SetValues(entity);

        return existingInfo;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var info = await _context.AdditionalInfos.FindAsync([id], cancellationToken);
        if (info == null)
            return false;

        _context.AdditionalInfos.Remove(info);
        return true;
    }
}
