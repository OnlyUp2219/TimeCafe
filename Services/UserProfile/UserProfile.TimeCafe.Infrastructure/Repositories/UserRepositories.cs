namespace UserProfile.TimeCafe.Infrastructure.Repositories;

public class UserRepositories(ApplicationDbContext context, HybridCache cache) : IUserRepositories
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<IEnumerable<Profile?>> GetAllAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Profile_All,
            async token => await _context.Profiles
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(token),
            tags: [CacheTags.Profiles],
            cancellationToken: ct);
    }

    public async Task<IEnumerable<Profile?>> GetPageAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Profile_Page(pageNumber),
            async token => await _context.Profiles
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(token),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Profiles],
            cancellationToken: ct);
    }

    public async Task<int> GetTotalPageAsync(CancellationToken ct = default)
    {
        return await _context.Profiles.CountAsync(ct);
    }

    public async Task<Profile?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Profile_ById(id),
            async token => await _context.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == id, token),
            tags: [CacheTags.Profiles, CacheTags.Profile(id)],
            cancellationToken: ct);
    }

    public async Task<IEnumerable<Profile>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        return await _context.Profiles
            .AsNoTracking()
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync(ct);
    }

    public async Task CreateEmptyAsync(Guid userId, CancellationToken ct = default)
    {
        var exist = await _context.Profiles
            .AnyAsync(u => u.UserId == userId, ct);
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


    public async Task<Profile> CreateAsync(Profile entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        _context.Profiles.Add(entity);
        return entity;
    }

    public async Task<Profile?> UpdateAsync(Profile entity, CancellationToken ct = default)
    {
        var existingClient = await _context.Profiles.FindAsync([entity.UserId], ct);
        if (existingClient is null)
            return null;

        _context.Entry(existingClient).CurrentValues.SetValues(entity);

        return existingClient;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var client = await _context.Profiles.FindAsync([id], ct);
        if (client is null)
            return false;

        _context.Profiles.Remove(client);
        return true;
    }
}
