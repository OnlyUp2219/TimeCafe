namespace Venue.TimeCafe.Infrastructure.Repositories;

public class PromotionRepository(
    ApplicationDbContext context,
    HybridCache cache) : IPromotionRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Promotion?> GetByIdAsync(Guid promotionId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Promotion_ById(promotionId),
            async cancellationToken => await _context.Promotions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PromotionId == promotionId, cancellationToken),
            tags: [CacheTags.Promotions, CacheTags.Promotion(promotionId)],
            cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Promotion>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<List<Promotion>>(
            CacheKeys.Promotion_All,
            async cancellationToken => await _context.Promotions
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken),
            tags: [CacheTags.Promotions],
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<IEnumerable<Promotion>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<List<Promotion>>(
            CacheKeys.Promotion_Active,
            async cancellationToken => await _context.Promotions
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken),
            tags: [CacheTags.Promotions],
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<IEnumerable<Promotion>> GetActiveByDateAsync(DateTimeOffset date, CancellationToken cancellationToken = default)
    {
        var dateKey = date.ToString("yyyy-MM-dd");
        return await _cache.GetOrCreateAsync<List<Promotion>>(
            CacheKeys.Promotion_ActiveByDate(dateKey),
            async cancellationToken => await _context.Promotions
                .AsNoTracking()
                .Where(p => p.IsActive && p.ValidFrom <= date && p.ValidTo >= date)
                .OrderByDescending(p => p.DiscountPercent)
                .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromHours(1) },
            tags: [CacheTags.Promotions],
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<IEnumerable<Promotion>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<List<Promotion>>(
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
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Promotions.CountAsync(cancellationToken);
    }

    public async Task<Promotion> CreateAsync(Promotion promotion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(promotion);

        promotion.CreatedAt = DateTimeOffset.UtcNow;
        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Promotions, cancellationToken);

        return promotion;
    }

    public async Task<Promotion> UpdateAsync(Promotion promotion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(promotion);

        var existingPromotion = await _context.Promotions.FindAsync([promotion.PromotionId], cancellationToken);
        if (existingPromotion == null)
            return null!;

        _context.Entry(existingPromotion).CurrentValues.SetValues(promotion);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Promotions, cancellationToken);

        return promotion;
    }

    public async Task<bool> DeleteAsync(Guid promotionId, CancellationToken cancellationToken = default)
    {
        var promotion = await _context.Promotions.FindAsync([promotionId], cancellationToken);
        if (promotion == null)
            return false;

        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Promotions, cancellationToken);

        return true;
    }

    public async Task<bool> ActivateAsync(Guid promotionId, CancellationToken cancellationToken = default)
    {
        var promotion = await _context.Promotions.FindAsync([promotionId], cancellationToken);
        if (promotion == null)
            return false;

        promotion.IsActive = true;
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Promotions, cancellationToken);

        return true;
    }

    public async Task<bool> DeactivateAsync(Guid promotionId, CancellationToken cancellationToken = default)
    {
        var promotion = await _context.Promotions.FindAsync([promotionId], cancellationToken);
        if (promotion == null)
            return false;

        promotion.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Promotions, cancellationToken);

        return true;
    }
}

