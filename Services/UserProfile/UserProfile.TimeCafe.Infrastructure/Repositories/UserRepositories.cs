namespace UserProfile.TimeCafe.Infrastructure.Repositories;

public class UserRepositories(ApplicationDbContext context, HybridCache cache) : IUserRepositories
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<IEnumerable<Profile?>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Profile_All,
            async token => await _context.Profiles
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(token),
            tags: [CacheTags.Profiles],
            cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Profile?>> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Profile_Page(pageNumber, pageSize),
            async token => await _context.Profiles
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(token),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Profiles],
            cancellationToken: cancellationToken);
    }

    public async Task<int> GetTotalPageAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Profiles.CountAsync(cancellationToken);
    }

    public async Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Profile_ById(id),
            async token => await _context.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == id, token),
            tags: [CacheTags.Profiles, CacheTags.Profile(id)],
            cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Profile>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        return await _context.Profiles
            .AsNoTracking()
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync(cancellationToken);
    }

    public async Task CreateEmptyAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var exist = await _context.Profiles
            .AnyAsync(u => u.UserId == userId, cancellationToken);
        if (exist)
            return;

        _context.Profiles.Add(new Profile()
        {
            UserId = userId,
            FirstName = "",
            LastName = "",
            Gender = Gender.NotSpecified,
            ProfileStatus = ProfileStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
        });
    }


    public async Task<Profile> CreateAsync(Profile entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        _context.Profiles.Add(entity);
        return entity;
    }

    public async Task<Profile?> UpdateAsync(Profile entity, CancellationToken cancellationToken = default)
    {
        var existingClient = await _context.Profiles.FindAsync([entity.UserId], cancellationToken);
        if (existingClient is null)
            return null;

        _context.Entry(existingClient).CurrentValues.SetValues(entity);

        return existingClient;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _context.Profiles.FindAsync([id], cancellationToken);
        if (client is null)
            return false;

        _context.Profiles.Remove(client);
        return true;
    }
}
