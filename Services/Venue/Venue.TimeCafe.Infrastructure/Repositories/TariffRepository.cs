namespace Venue.TimeCafe.Infrastructure.Repositories;

public class TariffRepository(ApplicationDbContext context, HybridCache cache) : ITariffRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Tariff?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Tariffs.AsNoTracking().FirstOrDefaultAsync(t => t.TariffId == id, cancellationToken);

    public async Task<TariffWithThemeDto?> GetWithThemeByIdAsync(Guid tariffId, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync(
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

    public async Task<IEnumerable<TariffWithThemeDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<TariffWithThemeDto>>(
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

    public async Task<IEnumerable<TariffWithThemeDto>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<TariffWithThemeDto>>(
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

    public async Task<IEnumerable<TariffWithThemeDto>> GetByBillingTypeAsync(BillingType billingType, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<TariffWithThemeDto>>(
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

    public async Task<IEnumerable<TariffWithThemeDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<TariffWithThemeDto>>(
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

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default) =>
        await _context.Tariffs.CountAsync(cancellationToken);

    public async Task<Tariff> CreateAsync(Tariff entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.LastModified = DateTimeOffset.UtcNow;
        _context.Tariffs.Add(entity);
        return entity;
    }

    public async Task<Tariff?> UpdateAsync(Tariff entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var existing = await _context.Tariffs.FindAsync([entity.TariffId], cancellationToken);
        if (existing == null) return null;

        entity.LastModified = DateTimeOffset.UtcNow;
        _context.Entry(existing).CurrentValues.SetValues(entity);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Tariffs.FindAsync([id], cancellationToken);
        if (entity == null) return false;

        _context.Tariffs.Remove(entity);
        return true;
    }

    public async Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Tariffs.FindAsync([id], cancellationToken);
        if (entity == null) return false;

        entity.IsActive = true;
        entity.LastModified = DateTimeOffset.UtcNow;
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Tariffs.FindAsync([id], cancellationToken);
        if (entity == null) return false;

        entity.IsActive = false;
        entity.LastModified = DateTimeOffset.UtcNow;
        return true;
    }

    public async Task<bool> AnyWithThemeIdAsync(Guid themeId, CancellationToken cancellationToken = default) =>
        await _context.Tariffs.AnyAsync(t => t.ThemeId == themeId, cancellationToken);
}
