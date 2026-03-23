namespace Venue.TimeCafe.Infrastructure.Repositories;

public class ThemeRepository(
    ApplicationDbContext context,
    HybridCache cache) : IThemeRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Theme?> GetByIdAsync(Guid themeId, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Theme_ById(themeId),
            async ct => await _context.Themes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ThemeId == themeId, ct),
            tags: [CacheTags.Themes, CacheTags.Theme(themeId)],
            cancellationToken: ct);
    }

    public async Task<IEnumerable<Theme>> GetAllAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync<List<Theme>>(
            CacheKeys.Theme_All,
            async ct => await _context.Themes
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync(ct),
            tags: [CacheTags.Themes],
            cancellationToken: ct) ?? [];
    }

    public async Task<Theme> CreateAsync(Theme theme, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(theme);

        _context.Themes.Add(theme);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Themes, ct);

        return theme;
    }

    public async Task<Theme> UpdateAsync(Theme theme, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(theme);

        var existingTheme = await _context.Themes.FindAsync([theme.ThemeId], ct);
        if (existingTheme == null)
            return null!;

        _context.Entry(existingTheme).CurrentValues.SetValues(theme);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Themes, ct);
        await _cache.RemoveByTagAsync(CacheTags.Tariffs, ct);

        return theme;
    }

    public async Task<bool> DeleteAsync(Guid themeId, CancellationToken ct = default)
    {
        var theme = await _context.Themes.FindAsync([themeId], ct);
        if (theme == null)
            return false;

        _context.Themes.Remove(theme);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Themes, ct);
        await _cache.RemoveByTagAsync(CacheTags.Tariffs, ct);

        return true;
    }
}
