using Venue.TimeCafe.Domain.DTOs;
using Venue.TimeCafe.Domain.Models;

namespace Venue.TimeCafe.Infrastructure.Repositories;

public class TariffRepository(
    ApplicationDbContext context,
    IDistributedCache cache,
    ILogger<TariffRepository> cacheLogger) : ITariffRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger _cacheLogger = cacheLogger;

    public async Task<TariffWithThemeDto?> GetByIdAsync(Guid tariffId)
    {
        var cached = await CacheHelper.GetAsync<TariffWithThemeDto>(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_ById(tariffId)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await (from t in _context.Tariffs
                            join th in _context.Themes on t.ThemeId equals th.ThemeId into thGroup
                            from th in thGroup.DefaultIfEmpty()
                            where t.TariffId == tariffId
                            select new TariffWithThemeDto
                            {
                                TariffId = t.TariffId,
                                TariffName = t.Name,
                                TariffDescription = t.Description,
                                TariffPricePerMinute = t.PricePerMinute,
                                TariffBillingType = t.BillingType,
                                TariffIsActive = t.IsActive,
                                TariffCreatedAt = t.CreatedAt,
                                TariffLastModified = t.LastModified,
                                ThemeId = th.ThemeId,
                                ThemeName = th != null ? th.Name : string.Empty,
                                ThemeEmoji = th != null ? th.Emoji : null,
                                ThemeColors = th != null ? th.Colors : null
                            })
                      .AsNoTracking().
                      FirstOrDefaultAsync().
                      ConfigureAwait(false);

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

    public async Task<IEnumerable<TariffWithThemeDto>> GetAllAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<TariffWithThemeDto>>(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_All).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await (from t in _context.Tariffs
                            join th in _context.Themes on t.ThemeId equals th.ThemeId into thGroup
                            from th in thGroup.DefaultIfEmpty()
                            orderby t.CreatedAt descending
                            select new TariffWithThemeDto
                            {
                                TariffId = t.TariffId,
                                TariffName = t.Name,
                                TariffDescription = t.Description,
                                TariffPricePerMinute = t.PricePerMinute,
                                TariffBillingType = t.BillingType,
                                TariffIsActive = t.IsActive,
                                TariffCreatedAt = t.CreatedAt,
                                TariffLastModified = t.LastModified,
                                ThemeId = th.ThemeId,
                                ThemeName = th != null ? th.Name : string.Empty,
                                ThemeEmoji = th != null ? th.Emoji : null,
                                ThemeColors = th != null ? th.Colors : null
                            })
                            .AsNoTracking()
                            .ToListAsync()
                            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_All,
            entity).ConfigureAwait(false);

        return entity;
    }

    public async Task<IEnumerable<TariffWithThemeDto>> GetActiveAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<TariffWithThemeDto>>(
        _cache,
        _cacheLogger,
        CacheKeys.Tariff_Active).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await (from t in _context.Tariffs
                            join th in _context.Themes on t.ThemeId equals th.ThemeId into thGroup
                            from th in thGroup.DefaultIfEmpty()
                            where t.IsActive
                            orderby t.Name
                            select new TariffWithThemeDto
                            {
                                TariffId = t.TariffId,
                                TariffName = t.Name,
                                TariffDescription = t.Description,
                                TariffPricePerMinute = t.PricePerMinute,
                                TariffBillingType = t.BillingType,
                                TariffIsActive = t.IsActive,
                                TariffCreatedAt = t.CreatedAt,
                                TariffLastModified = t.LastModified,
                                ThemeId = th.ThemeId,
                                ThemeName = th != null ? th.Name : string.Empty,
                                ThemeEmoji = th != null ? th.Emoji : null,
                                ThemeColors = th != null ? th.Colors : null
                            }
                            )
                            .AsNoTracking()
                            .ToListAsync()
                            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
        _cache,
        _cacheLogger,
        CacheKeys.Tariff_Active,
        entity).ConfigureAwait(false);

        return entity;
    }
    public async Task<IEnumerable<TariffWithThemeDto>> GetByBillingTypeAsync(BillingType billingType)
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<TariffWithThemeDto>>(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_ByBillingType((int)billingType)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entities = await (from t in _context.Tariffs
                              join th in _context.Themes on t.ThemeId equals th.ThemeId into thGroup
                              from th in thGroup.DefaultIfEmpty()
                              where t.BillingType == billingType && t.IsActive
                              orderby t.PricePerMinute
                              select new TariffWithThemeDto
                              {
                                  TariffId = t.TariffId,
                                  TariffName = t.Name,
                                  TariffDescription = t.Description,
                                  TariffPricePerMinute = t.PricePerMinute,
                                  TariffBillingType = t.BillingType,
                                  TariffIsActive = t.IsActive,
                                  TariffCreatedAt = t.CreatedAt,
                                  TariffLastModified = t.LastModified,
                                  ThemeId = th.ThemeId,
                                  ThemeName = th != null ? th.Name : string.Empty,
                                  ThemeEmoji = th != null ? th.Emoji : null,
                                  ThemeColors = th != null ? th.Colors : null
                              })
                              .AsNoTracking()
                              .ToListAsync()
                              .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Tariff_ByBillingType((int)billingType),
            entities).ConfigureAwait(false);

        return entities;
    }

    public async Task<IEnumerable<TariffWithThemeDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var versionStr = await _cache.GetStringAsync(CacheKeys.TariffPagesVersion()).ConfigureAwait(false);
        var version = int.TryParse(versionStr, out var v) ? v : 1;
        var cacheKey = CacheKeys.Tariff_Page(pageNumber, version);

        var cached = await CacheHelper.GetAsync<IEnumerable<TariffWithThemeDto>>(
            _cache,
            _cacheLogger,
            cacheKey).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var items = await (from t in _context.Tariffs
                           join th in _context.Themes on t.ThemeId equals th.ThemeId into thGroup
                           from th in thGroup.DefaultIfEmpty()
                           orderby t.CreatedAt descending
                           select new TariffWithThemeDto
                           {
                               TariffId = t.TariffId,
                               TariffName = t.Name,
                               TariffDescription = t.Description,
                               TariffPricePerMinute = t.PricePerMinute,
                               TariffBillingType = t.BillingType,
                               TariffIsActive = t.IsActive,
                               TariffCreatedAt = t.CreatedAt,
                               TariffLastModified = t.LastModified,
                               ThemeId = th.ThemeId,
                               ThemeName = th != null ? th.Name : string.Empty,
                               ThemeEmoji = th != null ? th.Emoji : null,
                               ThemeColors = th != null ? th.Colors : null
                           })
                           .AsNoTracking()
                           .Skip((pageNumber - 1) * pageSize)
                           .Take(pageSize)
                           .ToListAsync()
                           .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            cacheKey,
            items,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) })
            .ConfigureAwait(false);

        return items;
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Tariffs.CountAsync();
    }

    public async Task<Tariff> CreateAsync(Tariff tariff)
    {
        ArgumentNullException.ThrowIfNull(tariff);

        tariff.CreatedAt = DateTimeOffset.UtcNow;
        tariff.LastModified = DateTimeOffset.UtcNow;
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

        tariff.LastModified = DateTimeOffset.UtcNow;
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

    public async Task<bool> DeleteAsync(Guid tariffId)
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

    public async Task<bool> ActivateAsync(Guid tariffId)
    {
        var tariff = await _context.Tariffs.FindAsync(tariffId);
        if (tariff == null)
            return false;

        tariff.IsActive = true;
        tariff.LastModified = DateTimeOffset.UtcNow;
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

    public async Task<bool> DeactivateAsync(Guid tariffId)
    {
        var tariff = await _context.Tariffs.FindAsync(tariffId);
        if (tariff == null)
            return false;

        tariff.IsActive = false;
        tariff.LastModified = DateTimeOffset.UtcNow;
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
