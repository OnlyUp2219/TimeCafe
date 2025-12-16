namespace Venue.TimeCafe.Infrastructure.Repositories;

public class VisitRepository(
    ApplicationDbContext context,
    IDistributedCache cache,
    ILogger<VisitRepository> cacheLogger) : IVisitRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger _cacheLogger = cacheLogger;

    public async Task<VisitWithTariffDto?> GetByIdAsync(Guid visitId)
    {
        var cached = await CacheHelper.GetAsync<VisitWithTariffDto>(
            _cache,
            _cacheLogger,
            CacheKeys.Visit_ById(visitId)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await (from v in _context.Visits
                            join t in _context.Tariffs on v.TariffId equals t.TariffId into tGroup
                            from t in tGroup.DefaultIfEmpty()
                            where v.VisitId == visitId
                            select new VisitWithTariffDto
                            {
                                VisitId = v.VisitId,
                                UserId = v.UserId,
                                TariffId = v.TariffId,
                                EntryTime = v.EntryTime,
                                ExitTime = v.ExitTime,
                                CalculatedCost = v.CalculatedCost,
                                Status = v.Status,

                                TariffName = t != null ? t.Name : string.Empty,
                                TariffPricePerMinute = t != null ? t.PricePerMinute : 0m,
                                TariffDescription = t != null ? t.Description! : string.Empty
                            })
                    .AsNoTracking()
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

        if (entity != null)
        {
            await CacheHelper.SetAsync(
                _cache,
                _cacheLogger,
                CacheKeys.Visit_ById(visitId),
                entity).ConfigureAwait(false);
        }

        return entity;
    }

    public async Task<VisitWithTariffDto?> GetActiveVisitByUserAsync(Guid userId)
    {
        var cached = await CacheHelper.GetAsync<VisitWithTariffDto>(
            _cache,
            _cacheLogger,
            CacheKeys.Visit_ActiveByUser(userId)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var dto = await (from v in _context.Visits
                         join t in _context.Tariffs on v.TariffId equals t.TariffId into tGroup
                         from t in tGroup.DefaultIfEmpty()
                         where v.UserId == userId && v.Status == VisitStatus.Active
                         select new VisitWithTariffDto
                         {
                             VisitId = v.VisitId,
                             UserId = v.UserId,
                             TariffId = v.TariffId,
                             EntryTime = v.EntryTime,
                             ExitTime = v.ExitTime,
                             CalculatedCost = v.CalculatedCost,
                             Status = v.Status,

                             TariffName = t != null ? t.Name : string.Empty,
                             TariffPricePerMinute = t != null ? t.PricePerMinute : 0m,
                             TariffDescription = t != null ? t.Description! : string.Empty
                         })
                        .AsNoTracking()
                        .FirstOrDefaultAsync()
                        .ConfigureAwait(false);

        if (dto != null)
        {
            await CacheHelper.SetAsync(
                _cache,
                _cacheLogger,
                CacheKeys.Visit_ActiveByUser(userId),
                dto).ConfigureAwait(false);
        }

        return dto;
    }

    public async Task<IEnumerable<VisitWithTariffDto>> GetActiveVisitsAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<VisitWithTariffDto>>(
            _cache,
            _cacheLogger,
            CacheKeys.Visit_Active).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var items = await (from v in _context.Visits
                           join t in _context.Tariffs on v.TariffId equals t.TariffId into tGroup
                           from t in tGroup.DefaultIfEmpty()
                           where v.Status == VisitStatus.Active
                           orderby v.EntryTime descending
                           select new VisitWithTariffDto
                           {
                               VisitId = v.VisitId,
                               UserId = v.UserId,
                               TariffId = v.TariffId,
                               EntryTime = v.EntryTime,
                               ExitTime = v.ExitTime,
                               CalculatedCost = v.CalculatedCost,
                               Status = v.Status,

                               TariffName = t != null ? t.Name : string.Empty,
                               TariffPricePerMinute = t != null ? t.PricePerMinute : 0m,
                               TariffDescription = t != null ? t.Description! : string.Empty
                           })
                          .AsNoTracking()
                          .ToListAsync()
                          .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Visit_Active,
            items,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) }).ConfigureAwait(false);

        return items;
    }

    public async Task<IEnumerable<VisitWithTariffDto>> GetVisitHistoryByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
    {
        var cacheKey = CacheKeys.Visit_HistoryByUser(userId, pageNumber);
        var cached = await CacheHelper.GetAsync<IEnumerable<VisitWithTariffDto>>(
            _cache,
            _cacheLogger,
            cacheKey).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var items = await (from v in _context.Visits
                           join t in _context.Tariffs on v.TariffId equals t.TariffId into tGroup
                           from t in tGroup.DefaultIfEmpty()
                           where v.UserId == userId
                           orderby v.EntryTime descending
                           select new VisitWithTariffDto
                           {
                               VisitId = v.VisitId,
                               UserId = v.UserId,
                               TariffId = v.TariffId,
                               EntryTime = v.EntryTime,
                               ExitTime = v.ExitTime,
                               CalculatedCost = v.CalculatedCost,
                               Status = v.Status,

                               TariffName = t != null ? t.Name : string.Empty,
                               TariffPricePerMinute = t != null ? t.PricePerMinute : 0m,
                               TariffDescription = t != null ? t.Description! : string.Empty
                           })
                          .AsNoTracking()
                          .Skip((pageNumber - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync()
                          .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            cacheKey,
            items,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }).ConfigureAwait(false);

        return items;
    }

    public async Task<bool> HasActiveVisitAsync(Guid userId)
    {
        return await _context.Visits
            .AnyAsync(v => v.UserId == userId && v.Status == VisitStatus.Active);
    }

    public async Task<Visit> CreateAsync(Visit visit)
    {
        ArgumentNullException.ThrowIfNull(visit);
        visit.EntryTime = DateTime.UtcNow;
        visit.Status = VisitStatus.Active;
        _context.Visits.Add(visit);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Visit_Active,
            CacheKeys.Visit_ActiveByUser(visit.UserId),
            CacheKeys.Visit_ByUser(visit.UserId)).ConfigureAwait(false);

        return visit;
    }

    public async Task<Visit> UpdateAsync(Visit visit)
    {
        ArgumentNullException.ThrowIfNull(visit);
        var existingVisit = await _context.Visits.FindAsync(visit.VisitId);
        if (existingVisit == null)
            return null!;

        _context.Entry(existingVisit).CurrentValues.SetValues(visit);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Visit_Active,
            CacheKeys.Visit_ById(visit.VisitId),
            CacheKeys.Visit_ActiveByUser(visit.UserId),
            CacheKeys.Visit_ByUser(visit.UserId)).ConfigureAwait(false);

        return visit;
    }

    public async Task<bool> DeleteAsync(Guid visitId)
    {
        var visit = await _context.Visits.FindAsync(visitId);
        if (visit == null)
            return false;

        _context.Visits.Remove(visit);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Visit_Active,
            CacheKeys.Visit_ById(visitId),
            CacheKeys.Visit_ActiveByUser(visit.UserId),
            CacheKeys.Visit_ByUser(visit.UserId)).ConfigureAwait(false);

        return true;
    }
}