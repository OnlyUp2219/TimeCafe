namespace TimeCafe.Persistence.Repositories;

public class TariffRepository : ITariffRepository
{
    private readonly TimeCafeContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<TariffRepository> _logger;

    public TariffRepository(TimeCafeContext context, IDistributedCache cache, ILogger<TariffRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    public async Task<IEnumerable<Tariff>> GetAllTariffsAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<Tariff>>(
        _cache,
        _logger,
        CacheKeys.Tariff_All);
        if (cached != null)
            return cached;

        var entity = await _context.Tariffs
        .Include(t => t.BillingType)
        .Include(t => t.Theme)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();

        await CacheHelper.SetAsync(
        _cache,
        _logger,
        CacheKeys.Tariff_All,
        entity);

        return entity;
    }
    public async Task<IEnumerable<Tariff>> GetTariffsPageAsync(int pageNumber, int pageSize)
    {
        string versionStr = await _cache.GetStringAsync(CacheKeys.TariffPagesVersion());
        int version = int.TryParse(versionStr, out var v) ? v : 1;

        var cacheKey = CacheKeys.Tariff_Page(pageNumber, version);

        var cached = await CacheHelper.GetAsync<IEnumerable<Tariff>>(
            _cache,
            _logger,
            cacheKey);
        if (cached != null)
            return cached;

        var items = await _context.Tariffs
        .AsNoTracking()
        .Include(t => t.BillingType)
        .Include(t => t.Theme)
        .OrderByDescending(t => t.CreatedAt)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync()
        .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _logger,
            cacheKey,
            items,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

        return (items);
    }
    public async Task<int> GetTotalPageAsync()
    {
        return await _context.Tariffs.CountAsync();
    }
    public async Task<Tariff> GetTariffByIdAsync(int tariffId)
    {
        var cached = await CacheHelper.GetAsync<Tariff>(
        _cache,
        _logger,
        CacheKeys.Tariff_ById(tariffId));
        if (cached != null)
            return cached;

        var entity = await _context.Tariffs
        .Include(t => t.BillingType)
        .Include(t => t.Theme)
        .FirstOrDefaultAsync(t => t.TariffId == tariffId);

        await CacheHelper.SetAsync(
        _cache,
        _logger,
        CacheKeys.Tariff_ById(tariffId),
        entity);

        return entity;
    }


    public async Task<Tariff> CreateTariffAsync(Tariff tariff)
    {
        tariff.CreatedAt = DateTime.Now;
        tariff.LastModified = DateTime.Now;

        _context.Tariffs.Add(tariff);
        await _context.SaveChangesAsync();

        var removeAll = CacheHelper.RemoveKeysAsync(
            _cache,
            _logger,
            CacheKeys.Tariff_All);
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.TariffPagesVersion());

        Task.WaitAll(removeAll, removePage);

        return tariff;
    }
    public async Task<Tariff> UpdateTariffAsync(Tariff tariff)
    {
        var existingTariff = await _context.Tariffs.FindAsync(tariff.TariffId);
        if (existingTariff == null)
            throw new KeyNotFoundException($"Тариф с ID {tariff.TariffId} не найден");

        tariff.LastModified = DateTime.Now;
        _context.Entry(existingTariff).CurrentValues.SetValues(tariff);
        await _context.SaveChangesAsync();

        var removeAll = CacheHelper.RemoveKeysAsync(
            _cache,
            _logger,
            CacheKeys.Tariff_All,
            CacheKeys.Tariff_ById(tariff.TariffId));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.TariffPagesVersion());

        Task.WaitAll(removeAll, removePage);

        return tariff;
    }
    public async Task<bool> DeleteTariffAsync(int tariffId)
    {
        var tariff = await _context.Tariffs.FindAsync(tariffId);
        if (tariff == null)
            return false;

        _context.Tariffs.Remove(tariff);
        await _context.SaveChangesAsync();

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _logger,
            CacheKeys.Tariff_All,
            CacheKeys.Tariff_ById(tariffId));
        await CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.TariffPagesVersion());

        return true;
    }
}