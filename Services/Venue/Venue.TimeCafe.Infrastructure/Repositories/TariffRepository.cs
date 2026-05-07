namespace Venue.TimeCafe.Infrastructure.Repositories;

public class TariffRepository(
    ApplicationDbContext context,
    HybridCache cache) : ITariffRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<TariffWithThemeDto?> GetByIdAsync(Guid tariffId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Tariff_ById(tariffId),
            async cancellationToken => await (from t in _context.Tariffs
                               join th in _context.Themes on t.ThemeId equals th.ThemeId into thGroup
                               from th in thGroup.DefaultIfEmpty()
                               where t.TariffId == tariffId
                               select new TariffWithThemeDto
                               {
                                   TariffId = t.TariffId,
                                   Name = t.Name,
                                   Description = t.Description,
                                   PricePerMinute = t.PricePerMinute,
                                   BillingType = t.BillingType,
                                   IsActive = t.IsActive,
                                   CreatedAt = t.CreatedAt,
                                   LastModified = t.LastModified,
                                   ThemeId = th.ThemeId,
                                   ThemeName = th != null ? th.Name : string.Empty,
                                   ThemeEmoji = th != null ? th.Emoji : null,
                                   ThemeColors = th != null ? th.Colors : null
                               })
                              .AsNoTracking()
                              .FirstOrDefaultAsync(cancellationToken),
            tags: [CacheTags.Tariffs, CacheTags.Tariff(tariffId)],
            cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<TariffWithThemeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<List<TariffWithThemeDto>>(
            CacheKeys.Tariff_All,
            async cancellationToken => await (from t in _context.Tariffs
                               join th in _context.Themes on t.ThemeId equals th.ThemeId into thGroup
                               from th in thGroup.DefaultIfEmpty()
                               orderby t.CreatedAt descending
                               select new TariffWithThemeDto
                               {
                                   TariffId = t.TariffId,
                                   Name = t.Name,
                                   Description = t.Description,
                                   PricePerMinute = t.PricePerMinute,
                                   BillingType = t.BillingType,
                                   IsActive = t.IsActive,
                                   CreatedAt = t.CreatedAt,
                                   LastModified = t.LastModified,
                                   ThemeId = th.ThemeId,
                                   ThemeName = th != null ? th.Name : string.Empty,
                                   ThemeEmoji = th != null ? th.Emoji : null,
                                   ThemeColors = th != null ? th.Colors : null
                               })
                              .AsNoTracking()
                              .ToListAsync(cancellationToken),
            tags: [CacheTags.Tariffs],
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<IEnumerable<TariffWithThemeDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<List<TariffWithThemeDto>>(
            CacheKeys.Tariff_Active,
            async cancellationToken => await (from t in _context.Tariffs
                               join th in _context.Themes on t.ThemeId equals th.ThemeId into thGroup
                               from th in thGroup.DefaultIfEmpty()
                               where t.IsActive
                               orderby t.Name
                               select new TariffWithThemeDto
                               {
                                   TariffId = t.TariffId,
                                   Name = t.Name,
                                   Description = t.Description,
                                   PricePerMinute = t.PricePerMinute,
                                   BillingType = t.BillingType,
                                   IsActive = t.IsActive,
                                   CreatedAt = t.CreatedAt,
                                   LastModified = t.LastModified,
                                   ThemeId = th.ThemeId,
                                   ThemeName = th != null ? th.Name : string.Empty,
                                   ThemeEmoji = th != null ? th.Emoji : null,
                                   ThemeColors = th != null ? th.Colors : null
                               })
                              .AsNoTracking()
                              .ToListAsync(cancellationToken),
            tags: [CacheTags.Tariffs],
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<IEnumerable<TariffWithThemeDto>> GetByBillingTypeAsync(BillingType billingType, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<List<TariffWithThemeDto>>(
            CacheKeys.Tariff_ByBillingType((int)billingType),
            async cancellationToken => await (from t in _context.Tariffs
                               join th in _context.Themes on t.ThemeId equals th.ThemeId into thGroup
                               from th in thGroup.DefaultIfEmpty()
                               where t.BillingType == billingType && t.IsActive
                               orderby t.PricePerMinute
                               select new TariffWithThemeDto
                               {
                                   TariffId = t.TariffId,
                                   Name = t.Name,
                                   Description = t.Description,
                                   PricePerMinute = t.PricePerMinute,
                                   BillingType = t.BillingType,
                                   IsActive = t.IsActive,
                                   CreatedAt = t.CreatedAt,
                                   LastModified = t.LastModified,
                                   ThemeId = th.ThemeId,
                                   ThemeName = th != null ? th.Name : string.Empty,
                                   ThemeEmoji = th != null ? th.Emoji : null,
                                   ThemeColors = th != null ? th.Colors : null
                               })
                              .AsNoTracking()
                              .ToListAsync(cancellationToken),
            tags: [CacheTags.Tariffs],
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<IEnumerable<TariffWithThemeDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<List<TariffWithThemeDto>>(
            CacheKeys.Tariff_Page(pageNumber, pageSize),
            async cancellationToken => await (from t in _context.Tariffs
                               join th in _context.Themes on t.ThemeId equals th.ThemeId into thGroup
                               from th in thGroup.DefaultIfEmpty()
                               orderby t.CreatedAt descending
                               select new TariffWithThemeDto
                               {
                                   TariffId = t.TariffId,
                                   Name = t.Name,
                                   Description = t.Description,
                                   PricePerMinute = t.PricePerMinute,
                                   BillingType = t.BillingType,
                                   IsActive = t.IsActive,
                                   CreatedAt = t.CreatedAt,
                                   LastModified = t.LastModified,
                                   ThemeId = th.ThemeId,
                                   ThemeName = th != null ? th.Name : string.Empty,
                                   ThemeEmoji = th != null ? th.Emoji : null,
                                   ThemeColors = th != null ? th.Colors : null
                               })
                              .AsNoTracking()
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Tariffs],
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tariffs.CountAsync(cancellationToken);
    }

    public async Task<Tariff> CreateAsync(Tariff tariff, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tariff);

        tariff.CreatedAt = DateTimeOffset.UtcNow;
        tariff.LastModified = DateTimeOffset.UtcNow;
        _context.Tariffs.Add(tariff);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Tariffs, cancellationToken);

        return tariff;
    }

    public async Task<Tariff> UpdateAsync(Tariff tariff, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tariff);

        var existingTariff = await _context.Tariffs.FindAsync([tariff.TariffId], cancellationToken);
        if (existingTariff == null)
            return null!;

        tariff.LastModified = DateTimeOffset.UtcNow;
        _context.Entry(existingTariff).CurrentValues.SetValues(tariff);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Tariffs, cancellationToken);
        await _cache.RemoveByTagAsync(CacheTags.Visits, cancellationToken);

        return tariff;
    }

    public async Task<bool> DeleteAsync(Guid tariffId, CancellationToken cancellationToken = default)
    {
        var tariff = await _context.Tariffs.FindAsync([tariffId], cancellationToken);
        if (tariff == null)
            return false;

        _context.Tariffs.Remove(tariff);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Tariffs, cancellationToken);
        await _cache.RemoveByTagAsync(CacheTags.Visits, cancellationToken);

        return true;
    }

    public async Task<bool> ActivateAsync(Guid tariffId, CancellationToken cancellationToken = default)
    {
        var tariff = await _context.Tariffs.FindAsync([tariffId], cancellationToken);
        if (tariff == null)
            return false;

        tariff.IsActive = true;
        tariff.LastModified = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Tariffs, cancellationToken);

        return true;
    }

    public async Task<bool> DeactivateAsync(Guid tariffId, CancellationToken cancellationToken = default)
    {
        var tariff = await _context.Tariffs.FindAsync([tariffId], cancellationToken);
        if (tariff == null)
            return false;

        tariff.IsActive = false;
        tariff.LastModified = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Tariffs, cancellationToken);

        return true;
    }
}

