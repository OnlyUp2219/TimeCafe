namespace Venue.TimeCafe.Infrastructure.Repositories;

public class PromotionRepository(
    ApplicationDbContext context,
    IDistributedCache cache,
    ILogger<PromotionRepository> cacheLogger) : IPromotionRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger _cacheLogger = cacheLogger;

    public async Task<Promotion?> GetByIdAsync(int promotionId)
    {
        var cached = await CacheHelper.GetAsync<Promotion>(
            _cache,
            _cacheLogger,
            CacheKeys.Promotion_ById(promotionId)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Promotions
            .FirstOrDefaultAsync(p => p.PromotionId == promotionId)
            .ConfigureAwait(false);

        if (entity != null)
        {
            await CacheHelper.SetAsync(
                _cache,
                _cacheLogger,
                CacheKeys.Promotion_ById(promotionId),
                entity).ConfigureAwait(false);
        }

        return entity;
    }

    public async Task<IEnumerable<Promotion>> GetAllAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<Promotion>>(
            _cache,
            _cacheLogger,
            CacheKeys.Promotion_All).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Promotions
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync()
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Promotion_All,
            entity).ConfigureAwait(false);

        return entity;
    }

    public async Task<IEnumerable<Promotion>> GetActiveAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<Promotion>>(
            _cache,
            _cacheLogger,
            CacheKeys.Promotion_Active).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Promotions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync()
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Promotion_Active,
            entity).ConfigureAwait(false);

        return entity;
    }

    public async Task<IEnumerable<Promotion>> GetActiveByDateAsync(DateTime date)
    {
        var dateKey = date.ToString("yyyy-MM-dd");
        var cached = await CacheHelper.GetAsync<IEnumerable<Promotion>>(
            _cache,
            _cacheLogger,
            CacheKeys.Promotion_ActiveByDate(dateKey)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Promotions
            .AsNoTracking()
            .Where(p => p.IsActive && p.ValidFrom <= date && p.ValidTo >= date)
            .OrderByDescending(p => p.DiscountPercent)
            .ToListAsync()
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Promotion_ActiveByDate(dateKey),
            entity,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) }).ConfigureAwait(false);

        return entity;
    }

    public async Task<Promotion> CreateAsync(Promotion promotion)
    {
        ArgumentNullException.ThrowIfNull(promotion);

        promotion.CreatedAt = DateTime.UtcNow;
        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Promotion_All,
            CacheKeys.Promotion_Active).ConfigureAwait(false);

        return promotion;
    }

    public async Task<Promotion> UpdateAsync(Promotion promotion)
    {
        ArgumentNullException.ThrowIfNull(promotion);

        var existingPromotion = await _context.Promotions.FindAsync(promotion.PromotionId);
        if (existingPromotion == null)
            return null!;

        _context.Entry(existingPromotion).CurrentValues.SetValues(promotion);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Promotion_All,
            CacheKeys.Promotion_Active,
            CacheKeys.Promotion_ById(promotion.PromotionId)).ConfigureAwait(false);

        return promotion;
    }

    public async Task<bool> DeleteAsync(int promotionId)
    {
        var promotion = await _context.Promotions.FindAsync(promotionId);
        if (promotion == null)
            return false;

        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Promotion_All,
            CacheKeys.Promotion_Active,
            CacheKeys.Promotion_ById(promotionId)).ConfigureAwait(false);

        return true;
    }

    public async Task<bool> ActivateAsync(int promotionId)
    {
        var promotion = await _context.Promotions.FindAsync(promotionId);
        if (promotion == null)
            return false;

        promotion.IsActive = true;
        await _context.SaveChangesAsync().ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Promotion_All,
            CacheKeys.Promotion_Active,
            CacheKeys.Promotion_ById(promotionId)).ConfigureAwait(false);

        return true;
    }

    public async Task<bool> DeactivateAsync(int promotionId)
    {
        var promotion = await _context.Promotions.FindAsync(promotionId);
        if (promotion == null)
            return false;

        promotion.IsActive = false;
        await _context.SaveChangesAsync().ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Promotion_All,
            CacheKeys.Promotion_Active,
            CacheKeys.Promotion_ById(promotionId)).ConfigureAwait(false);

        return true;
    }
}
