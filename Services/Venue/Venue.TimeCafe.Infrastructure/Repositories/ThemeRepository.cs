namespace Venue.TimeCafe.Infrastructure.Repositories;

public class ThemeRepository(
    ApplicationDbContext context,
    IDistributedCache cache,
    ILogger<ThemeRepository> cacheLogger) : IThemeRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger _cacheLogger = cacheLogger;

    public async Task<Theme?> GetByIdAsync(int themeId)
    {
        var cached = await CacheHelper.GetAsync<Theme>(
            _cache,
            _cacheLogger,
            CacheKeys.Theme_ById(themeId)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Themes
            .FirstOrDefaultAsync(t => t.ThemeId == themeId)
            .ConfigureAwait(false);

        if (entity != null)
        {
            await CacheHelper.SetAsync(
                _cache,
                _cacheLogger,
                CacheKeys.Theme_ById(themeId),
                entity).ConfigureAwait(false);
        }

        return entity;
    }

    public async Task<IEnumerable<Theme>> GetAllAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<Theme>>(
            _cache,
            _cacheLogger,
            CacheKeys.Theme_All).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Themes
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync()
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Theme_All,
            entity).ConfigureAwait(false);

        return entity;
    }

    public async Task<Theme> CreateAsync(Theme theme)
    {
        _context.Themes.Add(theme);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Theme_All).ConfigureAwait(false);

        return theme;
    }

    public async Task<Theme> UpdateAsync(Theme theme)
    {
        var existingTheme = await _context.Themes.FindAsync(theme.ThemeId);
        if (existingTheme == null)
            return null!;

        _context.Entry(existingTheme).CurrentValues.SetValues(theme);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Theme_All,
            CacheKeys.Theme_ById(theme.ThemeId)).ConfigureAwait(false);

        return theme;
    }

    public async Task<bool> DeleteAsync(int themeId)
    {
        var theme = await _context.Themes.FindAsync(themeId);
        if (theme == null)
            return false;

        _context.Themes.Remove(theme);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Theme_All,
            CacheKeys.Theme_ById(themeId)).ConfigureAwait(false);

        return true;
    }
}
