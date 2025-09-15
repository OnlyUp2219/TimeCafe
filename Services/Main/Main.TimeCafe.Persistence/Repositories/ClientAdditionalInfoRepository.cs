namespace TimeCafe.Persistence.Repositories;

public class ClientAdditionalInfoRepository : IClientAdditionalInfoRepository
{
    private readonly TimeCafeContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ClientAdditionalInfoRepository> _logger;

    public ClientAdditionalInfoRepository(TimeCafeContext context, IDistributedCache cache, ILogger<ClientAdditionalInfoRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<ClientAdditionalInfo>> GetClientAdditionalInfosAsync(int clientId)
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<ClientAdditionalInfo>>(
            _cache,
            _logger,
            CacheKeys.ClientAdditionalInfo_All);
        if (cached != null)
            return cached;

        var entity = await _context.ClientAdditionalInfos
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        await CacheHelper.SetAsync(
            _cache,
            _logger,
            CacheKeys.ClientAdditionalInfo_All,
            entity);

        return entity;
    }

    public async Task<ClientAdditionalInfo> GetAdditionalInfoByIdAsync(int infoId)
    {
        var cached = await CacheHelper.GetAsync<ClientAdditionalInfo>(
            _cache,
            _logger,
            CacheKeys.ClientAdditionalInfo_ById(infoId));
        if (cached != null)
            return cached;

        var entity = await _context.ClientAdditionalInfos
            .Include(i => i.Client)
            .FirstOrDefaultAsync(i => i.InfoId == infoId);

        await CacheHelper.SetAsync(
             _cache,
             _logger,
             CacheKeys.ClientAdditionalInfo_ById(infoId),
             entity);

        return entity;
    }


    public async Task<ClientAdditionalInfo> CreateAdditionalInfoAsync(ClientAdditionalInfo info)
    {
        info.CreatedAt = DateTime.Now;
        _context.ClientAdditionalInfos.Add(info);
        await _context.SaveChangesAsync();

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _logger,
            CacheKeys.ClientAdditionalInfo_All);

        return info;
    }

    public async Task<ClientAdditionalInfo> UpdateAdditionalInfoAsync(ClientAdditionalInfo info)
    {
        var existingInfo = await _context.ClientAdditionalInfos.FindAsync(info.InfoId);
        if (existingInfo == null)
        {
            _logger.LogWarning("Попытка обновить ClientAdditionalInfo с несуществующим Id={Id}", info.InfoId);
            throw new KeyNotFoundException($"Дополнительная информация с ID {info.InfoId} не найдена");
        }

        _context.Entry(existingInfo).CurrentValues.SetValues(info);
        await _context.SaveChangesAsync();

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _logger,
            CacheKeys.ClientAdditionalInfo_All,
            CacheKeys.ClientAdditionalInfo_ById(info.InfoId)
        );

        return info;
    }

    public async Task<bool> DeleteAdditionalInfoAsync(int infoId)
    {
        var info = await _context.ClientAdditionalInfos.FindAsync(infoId);
        if (info == null)
            return false;

        _context.ClientAdditionalInfos.Remove(info);
        await _context.SaveChangesAsync();

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _logger,
            CacheKeys.ClientAdditionalInfo_All,
            CacheKeys.ClientAdditionalInfo_ById(info.InfoId)
        );

        return true;
    }
}