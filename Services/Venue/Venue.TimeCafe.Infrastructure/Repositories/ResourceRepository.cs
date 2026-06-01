namespace Venue.TimeCafe.Infrastructure.Repositories;

public class ResourceRepository(
    ApplicationDbContext context,
    HybridCache cache) : IResourceRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync(
            CacheKeys.Resource_ById(id),
            async cancellationToken => await _context.Resources
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ResourceId == id, cancellationToken),
            tags: [CacheTags.Resources, CacheTags.Resource(id)],
            cancellationToken: cancellationToken);

    public async Task<IEnumerable<Resource>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<Resource>>(
            CacheKeys.Resource_All,
            async cancellationToken => await _context.Resources
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken),
            tags: [CacheTags.Resources],
            cancellationToken: cancellationToken) ?? [];

    public async Task<IEnumerable<Resource>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<Resource>>(
            CacheKeys.Resource_Page(pageNumber, pageSize),
            async cancellationToken => await _context.Resources
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Resources],
            cancellationToken: cancellationToken) ?? [];

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default) =>
        await _context.Resources.CountAsync(cancellationToken);

    public async Task<Resource> CreateAsync(Resource resource, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);
        _context.Resources.Add(resource);
        return resource;
    }

    public async Task<Resource?> UpdateAsync(Resource resource, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);
        var existing = await _context.Resources.FindAsync([resource.ResourceId], cancellationToken);
        if (existing == null) return null;
        _context.Entry(existing).CurrentValues.SetValues(resource);
        return resource;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var resource = await _context.Resources.FindAsync([id], cancellationToken);
        if (resource == null) return false;
        _context.Resources.Remove(resource);
        return true;
    }
}
