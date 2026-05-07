namespace Venue.TimeCafe.Infrastructure.Repositories;

public class PromotionRepository(ApplicationDbContext context, HybridCache cache) : IPromotionRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync(
            CacheKeys.Promotion_ById(id),
            async cancellationToken => await _context.Promotions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PromotionId == id, cancellationToken),
            tags: [CacheTags.Promotions, CacheTags.Promotion(id)],
            cancellationToken: cancellationToken);

    public async Task<IEnumerable<Promotion>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<Promotion>>(
            CacheKeys.Promotion_All,
            async cancellationToken => await _context.Promotions
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken),
            tags: [CacheTags.Promotions],
            cancellationToken: cancellationToken) ?? [];

    public async Task<IEnumerable<Promotion>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<Promotion>>(
            CacheKeys.Promotion_Active,
            async cancellationToken => await _context.Promotions
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken),
            tags: [CacheTags.Promotions],
            cancellationToken: cancellationToken) ?? [];

    public async Task<IEnumerable<Promotion>> GetActiveByDateAsync(DateTimeOffset date, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<Promotion>>(
            CacheKeys.Promotion_ActiveByDate(date.ToString("yyyy-MM-dd")),
            async cancellationToken => await _context.Promotions
                .AsNoTracking()
                .Where(p => p.IsActive && p.ValidFrom <= date && p.ValidTo >= date)
                .OrderByDescending(p => p.DiscountPercent)
                .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromHours(1) },
            tags: [CacheTags.Promotions],
            cancellationToken: cancellationToken) ?? [];

    public async Task<IEnumerable<Promotion>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<Promotion>>(
            CacheKeys.Promotion_Page(pageNumber, pageSize),
            async cancellationToken => await _context.Promotions
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Promotions],
            cancellationToken: cancellationToken) ?? [];

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default) =>
        await _context.Promotions.CountAsync(cancellationToken);

    public async Task<Promotion> CreateAsync(Promotion entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.CreatedAt = DateTimeOffset.UtcNow;
        _context.Promotions.Add(entity);
        return entity;
    }

    public async Task<Promotion?> UpdateAsync(Promotion entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var existing = await _context.Promotions.FindAsync([entity.PromotionId], cancellationToken);
        if (existing == null) return null;
        _context.Entry(existing).CurrentValues.SetValues(entity);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Promotions.FindAsync([id], cancellationToken);
        if (entity == null) return false;
        _context.Promotions.Remove(entity);
        return true;
    }

    public async Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Promotions.FindAsync([id], cancellationToken);
        if (entity == null) return false;
        entity.IsActive = true;
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Promotions.FindAsync([id], cancellationToken);
        if (entity == null) return false;
        entity.IsActive = false;
        return true;
    }
}
