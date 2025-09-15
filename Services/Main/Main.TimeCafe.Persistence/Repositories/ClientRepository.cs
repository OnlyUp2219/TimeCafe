using TimeCafe.Core.Contracts.Services.Clients;
using TimeCafe.Core.Enums;

namespace TimeCafe.Persistence.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly Dictionary<int, bool> _confirmedPhones = new();
    private readonly TimeCafeContext _context;
    private readonly IClientUtilities _clientUtilities;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ClientRepository> _logger;

    public ClientRepository(TimeCafeContext context, IClientUtilities clientUtilities, IDistributedCache cache, ILogger<ClientRepository> logger)
    {
        _context = context;
        _clientUtilities = clientUtilities;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<Client>> GetAllClientsAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<Client>>(
            _cache,
            _logger,
            CacheKeys.Client_All);
        if (cached != null)
            return cached;

        var entity = await _context.Clients
            .Include(c => c.Status)
            .Include(c => c.Gender)
            .Include(c => c.ClientAdditionalInfos)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        await CacheHelper.SetAsync<IEnumerable<Client>>(
            _cache,
            _logger,
            CacheKeys.Client_All,
            entity);

        return entity;
    }

    public async Task<IEnumerable<Client>> GetClientsPageAsync(int pageNumber, int pageSize)
    {
        var versionStr = await _cache.GetStringAsync(CacheKeys.ClientPagesVersion());
        var version = int.TryParse(versionStr, out var v) ? v : 1;

        var cacheKey = CacheKeys.Client_Page(pageNumber, version);

        var cached = await CacheHelper.GetAsync<IEnumerable<Client>>(
            _cache,
            _logger,
            cacheKey);
        if (cached != null) return cached;

        var items = await _context.Clients
            .AsNoTracking()
            .Include(c => c.Status)
            .Include(c => c.Gender)
            .Include(c => c.ClientAdditionalInfos)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync()
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _logger,
            cacheKey,
            items,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

        return items;
    }

    public async Task<int> GetTotalPageAsync()
    {
        return await _context.Clients.CountAsync();
    }

    public async Task<Client> GetClientByIdAsync(int clientId)
    {
        var cached = await CacheHelper.GetAsync<Client>(
            _cache,
            _logger,
            CacheKeys.Client_ById(clientId));
        if (cached != null)
            return cached;

        var entity = await _context.Clients
            .Include(c => c.Status)
            .Include(c => c.Gender)
            .Include(c => c.ClientAdditionalInfos)
            .FirstOrDefaultAsync(c => c.ClientId == clientId);

        await CacheHelper.SetAsync(
            _cache,
            _logger,
            CacheKeys.Client_ById(clientId),
            entity);

        return entity;
    }

    public async Task<IEnumerable<ClientStatus>> GetClientStatusesAsync()
    {
        return await _context.ClientStatuses.ToListAsync();
    }

    public async Task<IEnumerable<Gender>> GetGendersAsync()
    {
        return await _context.Genders.ToListAsync();
    }

    public async Task<bool> IsPhoneConfirmedAsync(int clientId)
    {
        if (_confirmedPhones.TryGetValue(clientId, out var confirmed))
            return confirmed;

        var client = await _context.Clients.FindAsync(clientId);
        if (client == null)
            return false;

        var isConfirmed = client.StatusId == (int)EClientStatusType.Active;
        _confirmedPhones[clientId] = isConfirmed;
        return isConfirmed;
    }


    public async Task<Client> CreateClientAsync(Client client)
    {
        client.CreatedAt = DateTime.Now;
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        var removeAll = CacheHelper.RemoveKeysAsync(
            _cache,
            _logger,
            CacheKeys.Client_All);
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ClientPagesVersion());

        await Task.WhenAll(removeAll, removePage);

        return client;
    }

    public async Task<Client> UpdateClientAsync(Client client)
    {
        var existingClient = await _context.Clients.FindAsync(client.ClientId);
        if (existingClient == null)
            throw new KeyNotFoundException($"Клиент с ID {client.ClientId} не найден");

        _context.Entry(existingClient).CurrentValues.SetValues(client);
        await _context.SaveChangesAsync();

        var removeAll = CacheHelper.RemoveKeysAsync(
                   _cache,
                   _logger,
                   CacheKeys.Client_All,
                   CacheKeys.Client_ById(client.ClientId));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ClientPagesVersion());

        await Task.WhenAll(removeAll, removePage);

        return client;
    }

    public async Task<bool> DeleteClientAsync(int clientId)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null)
            return false;

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        var removeAll = CacheHelper.RemoveKeysAsync(
                   _cache,
                   _logger,
                   CacheKeys.Client_All,
                   CacheKeys.Client_ById(client.ClientId));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ClientPagesVersion());

        await Task.WhenAll(removeAll, removePage);

        return true;
    }

    public async Task<bool> UpdateClientStatusAsync(int clientId, int statusId)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null)
            return false;

        client.StatusId = statusId;
        await _context.SaveChangesAsync();

        var removeAll = CacheHelper.RemoveKeysAsync(
                   _cache,
                   _logger,
                   CacheKeys.Client_All,
                   CacheKeys.Client_ById(client.ClientId));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ClientPagesVersion());

        await Task.WhenAll(removeAll, removePage);

        return true;
    }

    public async Task<bool> SetClientActiveAsync(int clientId)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null)
            return false;

        client.StatusId = (int)EClientStatusType.Active;
        if (string.IsNullOrEmpty(client.AccessCardNumber))
        {
            client.AccessCardNumber = await _clientUtilities.GenerateAccessCardNumberAsync();
        }
        await _context.SaveChangesAsync();

        var removeAll = CacheHelper.RemoveKeysAsync(
                   _cache,
                   _logger,
                   CacheKeys.Client_All,
                   CacheKeys.Client_ById(client.ClientId));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ClientPagesVersion());

        await Task.WhenAll(removeAll, removePage);

        return true;
    }

    public async Task<bool> SetClientDraftAsync(int clientId)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null)
            return false;

        client.StatusId = (int)EClientStatusType.Draft;
        client.AccessCardNumber = null;
        await _context.SaveChangesAsync();

        var removeAll = CacheHelper.RemoveKeysAsync(
                   _cache,
                   _logger,
                   CacheKeys.Client_All,
                   CacheKeys.Client_ById(client.ClientId));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ClientPagesVersion());

        await Task.WhenAll(removeAll, removePage);

        return true;
    }

    public async Task<bool> SetClientRejectedAsync(int clientId, string reason)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null)
            return false;

        client.StatusId = (int)EClientStatusType.Rejected;
        client.RefusalReason = reason;
        client.AccessCardNumber = null;
        await _context.SaveChangesAsync();

        var removeAll = CacheHelper.RemoveKeysAsync(
            _cache,
            _logger,
            CacheKeys.Client_All,
            CacheKeys.Client_ById(client.ClientId));
        var removePage = CacheHelper.InvalidatePagesCacheAsync(_cache, CacheKeys.ClientPagesVersion());

        await Task.WhenAll(removeAll, removePage);

        return true;
    }
}