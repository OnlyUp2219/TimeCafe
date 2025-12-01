namespace Venue.TimeCafe.Infrastructure.Repositories;

public class VisitRepository(
    ApplicationDbContext context,
    IDistributedCache cache,
    ILogger<VisitRepository> cacheLogger) : IVisitRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger _cacheLogger = cacheLogger;

    public async Task<Visit?> GetByIdAsync(int visitId)
    {
        var cached = await CacheHelper.GetAsync<Visit>(
            _cache,
            _cacheLogger,
            CacheKeys.Visit_ById(visitId)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Visits
            .Include(v => v.Tariff)
            .FirstOrDefaultAsync(v => v.VisitId == visitId)
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

    public async Task<Visit?> GetActiveVisitByUserAsync(string userId)
    {
        var cached = await CacheHelper.GetAsync<Visit>(
            _cache,
            _cacheLogger,
            CacheKeys.Visit_ActiveByUser(userId)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Visits
            .Include(v => v.Tariff)
            .FirstOrDefaultAsync(v => v.UserId == userId && v.Status == VisitStatus.Active)
            .ConfigureAwait(false);

        if (entity != null)
        {
            await CacheHelper.SetAsync(
                _cache,
                _cacheLogger,
                CacheKeys.Visit_ActiveByUser(userId),
                entity).ConfigureAwait(false);
        }

        return entity;
    }

    public async Task<IEnumerable<Visit>> GetActiveVisitsAsync()
    {
        var cached = await CacheHelper.GetAsync<IEnumerable<Visit>>(
            _cache,
            _cacheLogger,
            CacheKeys.Visit_Active).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var entity = await _context.Visits
            .Include(v => v.Tariff)
            .AsNoTracking()
            .Where(v => v.Status == VisitStatus.Active)
            .OrderByDescending(v => v.EntryTime)
            .ToListAsync()
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Visit_Active,
            entity,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) }).ConfigureAwait(false);

        return entity;
    }

    public async Task<IEnumerable<Visit>> GetVisitHistoryByUserAsync(string userId, int pageNumber = 1, int pageSize = 20)
    {
        var cacheKey = CacheKeys.Visit_HistoryByUser(userId, pageNumber);
        var cached = await CacheHelper.GetAsync<IEnumerable<Visit>>(
            _cache,
            _cacheLogger,
            cacheKey).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var items = await _context.Visits
            .Include(v => v.Tariff)
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.EntryTime)
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

    public async Task<bool> HasActiveVisitAsync(string userId)
    {
        return await _context.Visits
            .AnyAsync(v => v.UserId == userId && v.Status == VisitStatus.Active);
    }

    public async Task<Visit> CreateAsync(Visit visit)
    {
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

    public async Task<bool> DeleteAsync(int visitId)
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