namespace Venue.TimeCafe.Infrastructure.Repositories;

public class ThemeRepository(
    ApplicationDbContext context,
    HybridCache cache) : IThemeRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Theme?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync(
            CacheKeys.Theme_ById(id),
            async cancellationToken => await _context.Themes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ThemeId == id, cancellationToken),
            tags: [CacheTags.Themes, CacheTags.Theme(id)],
            cancellationToken: cancellationToken);

    public async Task<IEnumerable<Theme>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<Theme>>(
            CacheKeys.Theme_All,
            async cancellationToken => await _context.Themes
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken),
            tags: [CacheTags.Themes],
            cancellationToken: cancellationToken) ?? [];

    public async Task<IEnumerable<Theme>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<Theme>>(
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

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default) =>
        await _context.Themes.CountAsync(cancellationToken);

    public async Task<Theme> CreateAsync(Theme theme, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(theme);
        _context.Themes.Add(theme);
        return theme;
    }

    public async Task<Theme?> UpdateAsync(Theme theme, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(theme);
        var existingTheme = await _context.Themes.FindAsync([theme.ThemeId], cancellationToken);
        if (existingTheme == null) return null;
        _context.Entry(existingTheme).CurrentValues.SetValues(theme);
        return theme;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var theme = await _context.Themes.FindAsync([id], cancellationToken);
        if (theme == null) return false;
        _context.Themes.Remove(theme);
        return true;
    }
}

