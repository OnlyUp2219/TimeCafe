namespace Main.TimeCafe.Infrastructure.Repositories;

public class ThemeRepository(TimeCafeContext context, IDistributedCache cache, ILogger<ThemeRepository> logger) : IThemeRepository
{
    private readonly TimeCafeContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<ThemeRepository> _logger = logger;

    public async Task<IEnumerable<Theme>> GetThemesAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<Theme>>(
            _cache,
            _logger,
            CacheKeys.Themes_All);
        if (cached != null)
            return cached;

        var entity = await _context.Themes
        .AsNoTracking()
        .ToListAsync();

        await CacheHelper.SetAsync(
            _cache,
            _logger,
            CacheKeys.Themes_All,
            entity);

        return entity;
    }
}