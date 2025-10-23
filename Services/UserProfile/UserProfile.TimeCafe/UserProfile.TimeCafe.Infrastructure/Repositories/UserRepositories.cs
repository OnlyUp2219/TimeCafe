using Microsoft.Extensions.Logging;

using UserProfile.TimeCafe.Domain.Contracts;

namespace UserProfile.TimeCafe.Infrastructure.Repositories;

public class UserRepositories : IUserRepositories
{

    //private readonly ApplicationDbContext _context;
    //private readonly IDistributedCache _cache;
    //private readonly ILogger<Profile> _logger;

    //public ClientRepository(ApplicationDbContext context, IClientUtilities clientUtilities, IDistributedCache cache, ILogger<ClientRepository> logger)
    //{
    //    _context = context;
    //    _clientUtilities = clientUtilities;
    //    _cache = cache;
    //    _logger = logger;
    //}

    //public async Task<IEnumerable<Profile>> GetAllProfilesAsync()
    //{
    //    var cached = await CacheHelper.GetAsync<IEnumerable<Profile>>(
    //        _cache,
    //        _logger,
    //        CacheKeys.Client_All);
    //    if (cached != null)
    //        return cached;

    //    var entity = await _context.Clients
    //        .Include(c => c.Status)
    //        .Include(c => c.Gender)
    //        .Include(c => c.ClientAdditionalInfos)
    //        .OrderByDescending(c => c.CreatedAt)
    //        .ToListAsync();

    //    await CacheHelper.SetAsync<IEnumerable<Profile>>(
    //        _cache,
    //        _logger,
    //        CacheKeys.Client_All,
    //        entity);

    //    return entity;
    //}

    //public async Task<IEnumerable<Profile>> GetProfilesPageAsync(int pageNumber, int pageSize)
    //{
    //    var versionStr = await _cache.GetStringAsync(CacheKeys.ClientPagesVersion());
    //    var version = int.TryParse(versionStr, out var v) ? v : 1;

    //    var cacheKey = CacheKeys.Client_Page(pageNumber, version);

    //    var cached = await CacheHelper.GetAsync<IEnumerable<Profile>>(
    //        _cache,
    //        _logger,
    //        cacheKey);
    //    if (cached != null) return cached;

    //    var items = await _context.Profiles
    //        .AsNoTracking()
    //        .Include(c => c.ClientAdditionalInfos)
    //        .OrderByDescending(c => c.CreatedAt)
    //        .Skip((pageNumber - 1) * pageSize)
    //        .Take(pageSize)
    //        .ToListAsync()
    //        .ConfigureAwait(false);

    //    await CacheHelper.SetAsync(
    //        _cache,
    //        _logger,
    //        cacheKey,
    //        items,
    //        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

    //    return items;
    //}

    //public async Task<int> GetTotalPageAsync()
    //{
    //    return await _context.Profiles.CountAsync();
    //}

    //public async Task<Profile> GetClientByIdAsync(int userId)
    //{
    //    var cached = await CacheHelper.GetAsync<Profile>(
    //        _cache,
    //        _logger,
    //        CacheKeys.Client_ById(clientId));
    //    if (cached != null)
    //        return cached;

    //    var entity = await _context.Profiles
    //        .Include(c => c.Status)
    //        .Include(c => c.Gender)
    //        .Include(c => c.ClientAdditionalInfos)
    //        .FirstOrDefaultAsync(c => c.ClientId == clientId);

    //    await CacheHelper.SetAsync(
    //        _cache,
    //        _logger,
    //        CacheKeys.Client_ById(clientId),
    //        entity);

    //    return entity;
    //}

    //public async Task<Profile> CreateProfileAsync(Profile client)
    //{
    //    client.CreatedAt = DateTime.Now;
    //    _context.Clients.Add(client);
    //    await _context.SaveChangesAsync();

    //    var removeAll = CacheHelper.RemoveKeysAsync(
    //        _cache,
    //        _logger,
    //        CacheKeys.Client_All);
    //    var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ClientPagesVersion());

    //    await Task.WhenAll(removeAll, removePage);

    //    return client;
    //}

    //public async Task<Profile> UpdateProfileAsync(Profile client)
    //{
    //    var existingClient = await _context.Clients.FindAsync(client.ClientId);
    //    if (existingClient == null)
    //        throw new KeyNotFoundException($"Клиент с ID {client.ClientId} не найден");

    //    _context.Entry(existingClient).CurrentValues.SetValues(client);
    //    await _context.SaveChangesAsync();

    //    var removeAll = CacheHelper.RemoveKeysAsync(
    //               _cache,
    //               _logger,
    //               CacheKeys.Client_All,
    //               CacheKeys.Client_ById(client.ClientId));
    //    var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ClientPagesVersion());

    //    await Task.WhenAll(removeAll, removePage);

    //    return client;
    //}

    //public async Task<bool> DeleteProfiletAsync(int userId)
    //{
    //    var client = await _context.Clients.FindAsync(clientId);
    //    if (client == null)
    //        return false;

    //    _context.Clients.Remove(client);
    //    await _context.SaveChangesAsync();

    //    var removeAll = CacheHelper.RemoveKeysAsync(
    //               _cache,
    //               _logger,
    //               CacheKeys.Client_All,
    //               CacheKeys.Client_ById(client.ClientId));
    //    var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ClientPagesVersion());

    //    await Task.WhenAll(removeAll, removePage);

    //    return true;
    //}

    //public async Task CreateEmptyAsync(string userId)
    //{
    //    var exist = await _context.Profile.Any(
    //}
    Task<Profile> IUserRepositories.CreateProfile(Profile client)
    {
        throw new NotImplementedException();
    }

    Task<bool> IUserRepositories.DeleteProfilet(int clientId)
    {
        throw new NotImplementedException();
    }

    Task<IEnumerable<Profile>> IUserRepositories.GetAllProfiles()
    {
        throw new NotImplementedException();
    }

    Task<Profile> IUserRepositories.GetClientById(int clientId)
    {
        throw new NotImplementedException();
    }

    Task<IEnumerable<Profile>> IUserRepositories.GetProfilesPage(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    Task<int> IUserRepositories.GetTotalPage()
    {
        throw new NotImplementedException();
    }

    Task<Profile> IUserRepositories.UpdateProfile(Profile client)
    {
        throw new NotImplementedException();
    }
}