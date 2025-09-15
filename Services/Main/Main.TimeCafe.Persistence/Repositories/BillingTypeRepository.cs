namespace TimeCafe.Persistence.Repositories;

public class BillingTypeRepository : IBillingTypeRepository
{
    private readonly TimeCafeContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<BillingTypeRepository> _logger;

    public BillingTypeRepository(TimeCafeContext context, IDistributedCache cache, ILogger<BillingTypeRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<BillingType>> GetBillingTypesAsync()
    {

        var cached = await CacheHelper.GetAsync<IEnumerable<BillingType>>(
            _cache,
            _logger,
            CacheKeys.BillingTypes_All);
        if (cached != null)
            return cached;

        var items = await _context.BillingTypes
            .AsNoTracking()
            .OrderBy(b => b.BillingTypeName)
            .ToListAsync();

        await CacheHelper.SetAsync<IEnumerable<BillingType>>(
            _cache,
            _logger,
            CacheKeys.BillingTypes_All,
            items);

        return items;
    }

    public async Task<BillingType> GetBillingTypeByIdAsync(int billingTypeId)
    {
        var cached = await CacheHelper.GetAsync<BillingType>(
            _cache,
            _logger,
            CacheKeys.BillingTypes_ById(billingTypeId)
            );
        if (cached != null)
            return cached;

        var entity = await _context.BillingTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BillingTypeId == billingTypeId);

        await CacheHelper.SetAsync<BillingType>(
            _cache,
            _logger,
            CacheKeys.BillingTypes_ById(billingTypeId),
            entity);

        return entity;
    }
}