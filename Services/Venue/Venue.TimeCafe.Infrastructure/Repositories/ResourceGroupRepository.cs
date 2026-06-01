namespace Venue.TimeCafe.Infrastructure.Repositories;

public class ResourceGroupRepository(
    ApplicationDbContext context,
    HybridCache cache) : IResourceGroupRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<ResourceGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync(
            CacheKeys.ResourceGroup_ById(id),
            async cancellationToken => await _context.ResourceGroups
                .AsNoTracking()
                .FirstOrDefaultAsync(rg => rg.ResourceGroupId == id, cancellationToken),
            tags: [CacheTags.ResourceGroups, CacheTags.ResourceGroup(id)],
            cancellationToken: cancellationToken);

    public async Task<IEnumerable<ResourceGroup>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<ResourceGroup>>(
            CacheKeys.ResourceGroup_All,
            async cancellationToken => await _context.ResourceGroups
                .AsNoTracking()
                .OrderBy(rg => rg.Name)
                .ToListAsync(cancellationToken),
            tags: [CacheTags.ResourceGroups],
            cancellationToken: cancellationToken) ?? [];

    public async Task<IEnumerable<ResourceGroup>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<ResourceGroup>>(
            CacheKeys.ResourceGroup_Page(pageNumber, pageSize),
            async cancellationToken => await _context.ResourceGroups
                .AsNoTracking()
                .OrderBy(rg => rg.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.ResourceGroups],
            cancellationToken: cancellationToken) ?? [];

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default) =>
        await _context.ResourceGroups.CountAsync(cancellationToken);

    public async Task<ResourceGroup> CreateAsync(ResourceGroup resourceGroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resourceGroup);
        _context.ResourceGroups.Add(resourceGroup);
        return resourceGroup;
    }

    public async Task<ResourceGroup?> UpdateAsync(ResourceGroup resourceGroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resourceGroup);
        var existing = await _context.ResourceGroups.FindAsync([resourceGroup.ResourceGroupId], cancellationToken);
        if (existing == null) return null;
        _context.Entry(existing).CurrentValues.SetValues(resourceGroup);
        return resourceGroup;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var resourceGroup = await _context.ResourceGroups.FindAsync([id], cancellationToken);
        if (resourceGroup == null) return false;
        _context.ResourceGroups.Remove(resourceGroup);
        return true;
    }
}
