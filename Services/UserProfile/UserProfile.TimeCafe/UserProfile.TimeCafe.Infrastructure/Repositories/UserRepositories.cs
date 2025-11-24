namespace UserProfile.TimeCafe.Infrastructure.Repositories;

public class UserRepositories(ApplicationDbContext context, IDistributedCache cache, ILogger<UserRepositories> logger) : IUserRepositories
{

    private readonly ApplicationDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<UserRepositories> _logger = logger;

    public async Task<IEnumerable<Profile?>> GetAllProfilesAsync(CancellationToken? cancellationToken)
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<Profile>>(
            _cache,
            _logger,
            CacheKeys.Profile_All).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Profiles
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken ?? CancellationToken.None)
            .ConfigureAwait(false);

        await CacheHelper.SetAsync<IEnumerable<Profile>>(
            _cache,
            _logger,
            CacheKeys.Profile_All,
            entity).ConfigureAwait(false);

        return entity;
    }

    public async Task<IEnumerable<Profile?>> GetProfilesPageAsync(int pageNumber, int pageSize, CancellationToken? cancellationToken)
    {
        var versionStr = await _cache.GetStringAsync(CacheKeys.ProfilePagesVersion()).ConfigureAwait(false);
        var version = int.TryParse(versionStr, out var v) ? v : 1;

        var cacheKey = CacheKeys.Profile_Page(pageNumber, version);

        var cached = await CacheHelper.GetAsync<IEnumerable<Profile>>(
            _cache,
            _logger,
            cacheKey).ConfigureAwait(false);
        if (cached != null) return cached;

        var items = await _context.Profiles
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken ?? CancellationToken.None)
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _logger,
            cacheKey,
            items,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }).ConfigureAwait(false);

        return items;
    }

    public async Task<int> GetTotalPageAsync(CancellationToken? cancellationToken)
    {
        return await _context.Profiles.CountAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
    }

    public async Task<Profile?> GetProfileByIdAsync(string userId, CancellationToken? cancellationToken)
    {
        var cached = await CacheHelper.GetAsync<Profile>(
            _cache,
            _logger,
            CacheKeys.Profile_ById(userId)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Profiles
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken ?? CancellationToken.None)
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _logger,
            CacheKeys.Profile_ById(userId),
            entity).ConfigureAwait(false);

        return entity;
    }

    public async Task<Profile?> CreateProfileAsync(Profile profile, CancellationToken? cancellationToken)
    {
        _logger.LogInformation("Создание профиля для UserId {UserId}", profile.UserId);
        profile.CreatedAt = DateTime.UtcNow;
        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

        var removeAll = CacheHelper.RemoveKeysAsync(
            _cache,
            _logger,
            CacheKeys.Profile_All);
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ProfilePagesVersion());
        _logger.LogInformation("Профиль для UserId {UserId} успешно создан", profile.UserId);

        await Task.WhenAll(removeAll, removePage).ConfigureAwait(false);

        return profile;
    }

    public async Task<Profile?> UpdateProfileAsync(Profile profile, CancellationToken? cancellationToken)
    {
        _logger.LogInformation("Обновление профиля для UserId {UserId}", profile.UserId);
        var existingClient = await _context.Profiles.FindAsync(profile.UserId);

        if (existingClient is null)
        {
            _logger.LogWarning("Профиль с UserId {UserId} не найден", profile.UserId);
            return null;
        }

        _context.Entry(existingClient).CurrentValues.SetValues(profile);
        await _context.SaveChangesAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        _logger.LogInformation("Профиль для UserId {UserId} успешно обновлен", profile.UserId);


        var removeAll = CacheHelper.RemoveKeysAsync(
                   _cache,
                   _logger,
                   CacheKeys.Profile_All,
                   CacheKeys.Profile_ById(profile.UserId));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ProfilePagesVersion());

        await Task.WhenAll(removeAll, removePage).ConfigureAwait(false);

        return profile;
    }

    public async Task DeleteProfileAsync(string userId, CancellationToken? cancellationToken)
    {
        _logger.LogInformation("Удаление профиля для UserId {UserId}", userId);
        var client = await _context.Profiles.FindAsync(userId).ConfigureAwait(false);

        if (client is null)
        {
            _logger.LogWarning("Профиль с UserId {UserId} не найден", userId);
            return;
        }

        _context.Profiles.Remove(client);
        await _context.SaveChangesAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        _logger.LogInformation("Профиль для UserId {UserId} успешно удален", userId);

        var removeAll = CacheHelper.RemoveKeysAsync(
                   _cache,
                   _logger,
                   CacheKeys.Profile_All,
                   CacheKeys.Profile_ById(client.UserId));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ProfilePagesVersion());

        await Task.WhenAll(removeAll, removePage).ConfigureAwait(false);
    }

    public async Task CreateEmptyAsync(string userId, CancellationToken? cancellationToken)
    {
        _logger.LogInformation("Создание пустого профиля для UserId {UserId}", userId);
        var exist = await _context.Profiles
            .AnyAsync(u => u.UserId == userId, cancellationToken ?? CancellationToken.None)
            .ConfigureAwait(false);
        if (exist)
            return;

        _context.Profiles.Add(new Profile()
        {
            UserId = userId,
            FirstName = "",
            LastName = "",
            Gender = Gender.NotSpecified,
            ProfileStatus = ProfileStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        _logger.LogInformation("Пустой профиль для UserId {UserId} успешно создан", userId);

        var removeAll = CacheHelper.RemoveKeysAsync(
        _cache,
        _logger,
        CacheKeys.Profile_All);
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ProfilePagesVersion());
        await Task.WhenAll(removeAll, removePage).ConfigureAwait(false);

    }

}