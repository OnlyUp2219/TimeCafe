namespace Venue.TimeCafe.Infrastructure.Repositories;

public class TariffRepository(
    ApplicationDbContext context,
    IDistributedCache cache,
    ILogger<TariffRepository> cacheLogger) : ITariffRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger _cacheLogger = cacheLogger;

    public async Task<Tariff?> GetByIdAsync(int tariffId)
    {
        var cached = await CacheHelper.GetAsync<Tariff>(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_ById(tariffId)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Tariffs
            .Include(t => t.Theme)
            .FirstOrDefaultAsync(t => t.TariffId == tariffId)
            .ConfigureAwait(false);

        if (entity != null)
        {
            await CacheHelper.SetAsync(
                _cache,
                _cacheLogger,
                CacheKeys.Tariff_ById(tariffId),
                entity).ConfigureAwait(false);
        }

        return entity;
    }

    public async Task<IEnumerable<Tariff>> GetAllAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<Tariff>>(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_All).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Tariffs
            .Include(t => t.Theme)
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync()
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_All,
            entity).ConfigureAwait(false);

        return entity;
    }

    public async Task<IEnumerable<Tariff>> GetActiveAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<Tariff>>(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_Active).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Tariffs
            .Include(t => t.Theme)
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync()
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_Active,
            entity).ConfigureAwait(false);

        return entity;
    }

    public async Task<IEnumerable<Tariff>> GetByBillingTypeAsync(BillingType billingType)
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<Tariff>>(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_ByBillingType((int)billingType)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Tariffs
            .Include(t => t.Theme)
            .AsNoTracking()
            .Where(t => t.BillingType == billingType && t.IsActive)
            .OrderBy(t => t.PricePerMinute)
            .ToListAsync()
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_ByBillingType((int)billingType),
            entity).ConfigureAwait(false);

        return entity;
    }

    public async Task<IEnumerable<Tariff>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var versionStr = await _cache.GetStringAsync(CacheKeys.TariffPagesVersion()).ConfigureAwait(false);
        var version = int.TryParse(versionStr, out var v) ? v : 1;
        var cacheKey = CacheKeys.Tariff_Page(pageNumber, version);

        var cached = await CacheHelper.GetAsync<IEnumerable<Tariff>>(
            _cache,
            _cacheLogger,
            cacheKey).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var items = await _context.Tariffs
            .Include(t => t.Theme)
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync()
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            cacheKey,
            items,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }).ConfigureAwait(false);

        return items;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Tariffs.CountAsync();
    }

    public async Task<Tariff> CreateAsync(Tariff tariff)
    {
        ArgumentNullException.ThrowIfNull(tariff);

        tariff.CreatedAt = DateTime.UtcNow;
        tariff.LastModified = DateTime.UtcNow;
        _context.Tariffs.Add(tariff);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        var removeAll = CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_All,
            CacheKeys.Tariff_Active,
            CacheKeys.Tariff_ByBillingType((int)tariff.BillingType));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.TariffPagesVersion());
        await Task.WhenAll(removeAll, removePage).ConfigureAwait(false);

        return tariff;
    }

    public async Task<Tariff> UpdateAsync(Tariff tariff)
    {
        ArgumentNullException.ThrowIfNull(tariff);

        var existingTariff = await _context.Tariffs.FindAsync(tariff.TariffId);
        if (existingTariff == null)
            return null!;

        tariff.LastModified = DateTime.UtcNow;
        _context.Entry(existingTariff).CurrentValues.SetValues(tariff);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        var removeAll = CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_All,
            CacheKeys.Tariff_Active,
            CacheKeys.Tariff_ById(tariff.TariffId),
            CacheKeys.Tariff_ByBillingType((int)tariff.BillingType));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.TariffPagesVersion());
        await Task.WhenAll(removeAll, removePage).ConfigureAwait(false);

        return tariff;
    }

    public async Task<bool> DeleteAsync(int tariffId)
    {
        var tariff = await _context.Tariffs.FindAsync(tariffId);
        if (tariff == null)
            return false;

        _context.Tariffs.Remove(tariff);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        var removeAll = CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_All,
            CacheKeys.Tariff_Active,
            CacheKeys.Tariff_ById(tariffId),
            CacheKeys.Tariff_ByBillingType((int)tariff.BillingType));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.TariffPagesVersion());
        await Task.WhenAll(removeAll, removePage).ConfigureAwait(false);

        return true;
    }

    public async Task<bool> ActivateAsync(int tariffId)
    {
        var tariff = await _context.Tariffs.FindAsync(tariffId);
        if (tariff == null)
            return false;

        tariff.IsActive = true;
        tariff.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync().ConfigureAwait(false);

        var removeAll = CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_All,
            CacheKeys.Tariff_Active,
            CacheKeys.Tariff_ById(tariffId),
            CacheKeys.Tariff_ByBillingType((int)tariff.BillingType));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.TariffPagesVersion());
        await Task.WhenAll(removeAll, removePage).ConfigureAwait(false);

        return true;
    }

    public async Task<bool> DeactivateAsync(int tariffId)
    {
        var tariff = await _context.Tariffs.FindAsync(tariffId);
        if (tariff == null)
            return false;

        tariff.IsActive = false;
        tariff.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync().ConfigureAwait(false);

        var removeAll = CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_All,
            CacheKeys.Tariff_Active,
            CacheKeys.Tariff_ById(tariffId),
            CacheKeys.Tariff_ByBillingType((int)tariff.BillingType));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.TariffPagesVersion());
        await Task.WhenAll(removeAll, removePage).ConfigureAwait(false);

        return true;
    }
}
