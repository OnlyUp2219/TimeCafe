namespace Venue.TimeCafe.Infrastructure.Repositories;

public class PromotionRepository(
    ApplicationDbContext context,
    HybridCache cache) : IPromotionRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Promotion?> GetByIdAsync(Guid promotionId, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Promotion_ById(promotionId),
            async ct => await _context.Promotions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PromotionId == promotionId, ct),
            tags: [CacheTags.Promotions, CacheTags.Promotion(promotionId)],
            cancellationToken: ct);
    }

    public async Task<IEnumerable<Promotion>> GetAllAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync<List<Promotion>>(
            CacheKeys.Promotion_All,
            async ct => await _context.Promotions
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct),
            tags: [CacheTags.Promotions],
            cancellationToken: ct) ?? [];
    }

    public async Task<IEnumerable<Promotion>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync<List<Promotion>>(
            CacheKeys.Promotion_Active,
            async ct => await _context.Promotions
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct),
            tags: [CacheTags.Promotions],
            cancellationToken: ct) ?? [];
    }

    public async Task<IEnumerable<Promotion>> GetActiveByDateAsync(DateTimeOffset date, CancellationToken ct = default)
    {
        var dateKey = date.ToString("yyyy-MM-dd");
        return await _cache.GetOrCreateAsync<List<Promotion>>(
            CacheKeys.Promotion_ActiveByDate(dateKey),
            async ct => await _context.Promotions
                .AsNoTracking()
                .Where(p => p.IsActive && p.ValidFrom <= date && p.ValidTo >= date)
                .OrderByDescending(p => p.DiscountPercent)
                .ToListAsync(ct),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromHours(1) },
            tags: [CacheTags.Promotions],
            cancellationToken: ct) ?? [];
    }

    public async Task<IEnumerable<Promotion>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync<List<Promotion>>(
            CacheKeys.Promotion_Page(pageNumber, pageSize),
            async ct => await _context.Promotions
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Promotions],
            cancellationToken: ct) ?? [];
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
    {
        return await _context.Promotions.CountAsync(ct);
    }

    public async Task<Promotion> CreateAsync(Promotion promotion, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(promotion);

        promotion.CreatedAt = DateTimeOffset.UtcNow;
        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Promotions, ct);

        return promotion;
    }

    public async Task<Promotion> UpdateAsync(Promotion promotion, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(promotion);

        var existingPromotion = await _context.Promotions.FindAsync([promotion.PromotionId], ct);
        if (existingPromotion == null)
            return null!;

        _context.Entry(existingPromotion).CurrentValues.SetValues(promotion);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Promotions, ct);

        return promotion;
    }

    public async Task<bool> DeleteAsync(Guid promotionId, CancellationToken ct = default)
    {
        var promotion = await _context.Promotions.FindAsync([promotionId], ct);
        if (promotion == null)
            return false;

        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Promotions, ct);

        return true;
    }

    public async Task<bool> ActivateAsync(Guid promotionId, CancellationToken ct = default)
    {
        var promotion = await _context.Promotions.FindAsync([promotionId], ct);
        if (promotion == null)
            return false;

        promotion.IsActive = true;
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Promotions, ct);

        return true;
    }

    public async Task<bool> DeactivateAsync(Guid promotionId, CancellationToken ct = default)
    {
        var promotion = await _context.Promotions.FindAsync([promotionId], ct);
        if (promotion == null)
            return false;

        promotion.IsActive = false;
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Promotions, ct);

        return true;
    }
}

