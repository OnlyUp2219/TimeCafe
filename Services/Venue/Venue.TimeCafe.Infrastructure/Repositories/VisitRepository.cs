namespace Venue.TimeCafe.Infrastructure.Repositories;

public class VisitRepository(
    ApplicationDbContext context,
    HybridCache cache) : IVisitRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<VisitWithTariffDto?> GetByIdAsync(Guid visitId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Visit_ById(visitId),
            async cancellationToken => await (from v in _context.Visits
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
                              .FirstOrDefaultAsync(cancellationToken),
            tags: [CacheTags.Visits, CacheTags.Visit(visitId)],
            cancellationToken: cancellationToken);
    }

    public async Task<VisitWithTariffDto?> GetActiveVisitByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Visit_ActiveByUser(userId),
            async cancellationToken => await (from v in _context.Visits
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
                              .FirstOrDefaultAsync(cancellationToken),
            tags: [CacheTags.Visits, CacheTags.VisitByUser(userId)],
            cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<VisitWithTariffDto>> GetActiveVisitsAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<List<VisitWithTariffDto>>(
            CacheKeys.Visit_Active,
            async cancellationToken => await (from v in _context.Visits
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
                              .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
            tags: [CacheTags.Visits],
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<IEnumerable<VisitWithTariffDto>> GetVisitHistoryByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<List<VisitWithTariffDto>>(
            CacheKeys.Visit_HistoryByUser(userId, pageNumber, pageSize),
            async cancellationToken => await (from v in _context.Visits
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
                              .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(10) },
            tags: [CacheTags.Visits, CacheTags.VisitByUser(userId)],
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<IEnumerable<VisitWithTariffDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync<List<VisitWithTariffDto>>(
            CacheKeys.Visit_Page(pageNumber, pageSize),
            async cancellationToken => await (from v in _context.Visits
                               join t in _context.Tariffs on v.TariffId equals t.TariffId into tGroup
                               from t in tGroup.DefaultIfEmpty()
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
                              .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Visits],
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Visits.CountAsync(cancellationToken);
    }

    public async Task<bool> HasActiveVisitAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Visits
            .AnyAsync(v => v.UserId == userId && v.Status == VisitStatus.Active, cancellationToken);
    }

    public async Task<Visit> CreateAsync(Visit visit, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(visit);
        visit.EntryTime = DateTimeOffset.UtcNow;
        visit.Status = VisitStatus.Active;
        _context.Visits.Add(visit);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Visits, cancellationToken);

        return visit;
    }

    public async Task<Visit> UpdateAsync(Visit visit, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(visit);
        var existingVisit = await _context.Visits.FindAsync([visit.VisitId], cancellationToken);
        if (existingVisit == null)
            return null!;

        _context.Entry(existingVisit).CurrentValues.SetValues(visit);

        if (saveChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);

            await _cache.RemoveByTagAsync(CacheTags.Visits, cancellationToken);
        }

        return visit;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid visitId, CancellationToken cancellationToken = default)
    {
        var visit = await _context.Visits.FindAsync([visitId], cancellationToken);
        if (visit == null)
            return false;

        _context.Visits.Remove(visit);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync(CacheTags.Visits, cancellationToken);

        return true;
    }
}

