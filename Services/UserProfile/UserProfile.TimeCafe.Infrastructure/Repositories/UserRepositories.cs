namespace UserProfile.TimeCafe.Infrastructure.Repositories;

public class UserRepositories(ApplicationDbContext context, HybridCache cache) : IUserRepositories
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<IEnumerable<Profile?>> GetAllProfilesAsync(CancellationToken cancellationToken = default)
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

    public async Task<IEnumerable<Profile?>> GetProfilesPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
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
            cancellationToken: cancellationToken);
    }

    public async Task<int> GetTotalPageAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Profiles.CountAsync(cancellationToken);
    }

    public async Task<Profile?> GetProfileByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Profile_ById(userId),
            async token => await _context.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId, token),
            tags: [CacheTags.Profiles, CacheTags.Profile(userId)],
            cancellationToken: cancellationToken);
    }

    public async Task<Profile?> CreateProfileAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        profile.CreatedAt = DateTimeOffset.UtcNow;
        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Profiles, cancellationToken);

        return profile;
    }

    public async Task<Profile?> UpdateProfileAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        var existingClient = await _context.Profiles.FindAsync([profile.UserId], cancellationToken);

        if (existingClient is null)
            return null;

        _context.Entry(existingClient).CurrentValues.SetValues(profile);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Profiles, cancellationToken);

        return profile;
    }

    public async Task DeleteProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var client = await _context.Profiles.FindAsync([userId], cancellationToken);

        if (client is null)
            return;

        _context.Profiles.Remove(client);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Profiles, cancellationToken);
        await _cache.RemoveByTagAsync(CacheTags.AdditionalInfos, cancellationToken);
        await _cache.RemoveByTagAsync(CacheTags.AdditionalInfoByUser(userId), cancellationToken);
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

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Profiles, cancellationToken);
    }

    public async Task<IEnumerable<Profile>> GetProfilesByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        return await _context.Profiles
            .AsNoTracking()
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync(cancellationToken);
    }
}
