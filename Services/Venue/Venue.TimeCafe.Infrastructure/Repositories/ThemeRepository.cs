namespace Venue.TimeCafe.Infrastructure.Repositories;

public class ThemeRepository(
    ApplicationDbContext context,
    HybridCache cache) : IThemeRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Theme?> GetByIdAsync(Guid themeId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Theme_ById(themeId),
            async cancellationToken => await _context.Themes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ThemeId == themeId, cancellationToken),
            tags: [CacheTags.Themes, CacheTags.Theme(themeId)],
            cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Theme>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<List<Theme>>(
            CacheKeys.Theme_All,
            async cancellationToken => await _context.Themes
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken),
            tags: [CacheTags.Themes],
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<IEnumerable<Theme>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<List<Theme>>(
            CacheKeys.Theme_Page(pageNumber, pageSize),
            async cancellationToken => await _context.Themes
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Themes],
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Themes.CountAsync(cancellationToken);
    }

    public async Task<Theme> CreateAsync(Theme theme, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(theme);

        _context.Themes.Add(theme);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Themes, cancellationToken);

        return theme;
    }

    public async Task<Theme> UpdateAsync(Theme theme, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(theme);

        var existingTheme = await _context.Themes.FindAsync([theme.ThemeId], cancellationToken);
        if (existingTheme == null)
            return null!;

        _context.Entry(existingTheme).CurrentValues.SetValues(theme);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Themes, cancellationToken);
        await _cache.RemoveByTagAsync(CacheTags.Tariffs, cancellationToken);

        return theme;
    }

    public async Task<bool> DeleteAsync(Guid themeId, CancellationToken cancellationToken = default)
    {
        var theme = await _context.Themes.FindAsync([themeId], cancellationToken);
        if (theme == null)
            return false;

        _context.Themes.Remove(theme);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Themes, cancellationToken);
        await _cache.RemoveByTagAsync(CacheTags.Tariffs, cancellationToken);

        return true;
    }
}

