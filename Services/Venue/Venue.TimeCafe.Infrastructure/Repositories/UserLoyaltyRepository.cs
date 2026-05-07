namespace Venue.TimeCafe.Infrastructure.Repositories;

public class UserLoyaltyRepository(ApplicationDbContext context, HybridCache cache) : IUserLoyaltyRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<UserLoyalty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync(
            CacheKeys.UserLoyalty_ByUserId(id),
            async cancellationToken => await _context.UserLoyalties.FirstOrDefaultAsync(x => x.UserId == id, cancellationToken),
            tags: [CacheTags.UserLoyalties, CacheTags.UserLoyalty(id)],
            cancellationToken: cancellationToken);

    public async Task<UserLoyalty?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await GetByIdAsync(userId, cancellationToken);

    public async Task<UserLoyalty> CreateAsync(UserLoyalty entity, CancellationToken cancellationToken = default)
    {
        _context.UserLoyalties.Add(entity);
        return entity;
    }

    public async Task<UserLoyalty?> UpdateAsync(UserLoyalty entity, CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserLoyalties.FindAsync([entity.UserId], cancellationToken);
        if (existing == null) return null;

        entity.LastUpdated = DateTimeOffset.UtcNow;
        _context.Entry(existing).CurrentValues.SetValues(entity);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserLoyalties.FindAsync([id], cancellationToken);
        if (entity == null) return false;

        _context.UserLoyalties.Remove(entity);
        return true;
    }
}

