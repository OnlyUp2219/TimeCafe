namespace Venue.TimeCafe.Infrastructure.Repositories;

public class VisitRepository(
    ApplicationDbContext context,
    HybridCache cache) : IVisitRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<VisitWithTariffDto?> GetByIdAsync(Guid visitId, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Visit_ById(visitId),
            async ct => await (from v in _context.Visits
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
                              .FirstOrDefaultAsync(ct),
            tags: [CacheTags.Visits, CacheTags.Visit(visitId)],
            cancellationToken: ct);
    }

    public async Task<VisitWithTariffDto?> GetActiveVisitByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Visit_ActiveByUser(userId),
            async ct => await (from v in _context.Visits
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
                              .FirstOrDefaultAsync(ct),
            tags: [CacheTags.Visits, CacheTags.VisitByUser(userId)],
            cancellationToken: ct);
    }

    public async Task<IEnumerable<VisitWithTariffDto>> GetActiveVisitsAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync<List<VisitWithTariffDto>>(
            CacheKeys.Visit_Active,
            async ct => await (from v in _context.Visits
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
                              .ToListAsync(ct),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
            tags: [CacheTags.Visits],
            cancellationToken: ct) ?? [];
    }

    public async Task<IEnumerable<VisitWithTariffDto>> GetVisitHistoryByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync<List<VisitWithTariffDto>>(
            CacheKeys.Visit_HistoryByUser(userId, pageNumber),
            async ct => await (from v in _context.Visits
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
                              .ToListAsync(ct),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(10) },
            tags: [CacheTags.Visits, CacheTags.VisitByUser(userId)],
            cancellationToken: ct) ?? [];
    }

    public async Task<bool> HasActiveVisitAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Visits
            .AnyAsync(v => v.UserId == userId && v.Status == VisitStatus.Active, ct);
    }

    public async Task<Visit> CreateAsync(Visit visit, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(visit);
        visit.EntryTime = DateTimeOffset.UtcNow;
        visit.Status = VisitStatus.Active;
        _context.Visits.Add(visit);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Visits, ct);

        return visit;
    }

    public async Task<Visit> UpdateAsync(Visit visit, bool saveChanges = true, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(visit);
        var existingVisit = await _context.Visits.FindAsync([visit.VisitId], ct);
        if (existingVisit == null)
            return null!;

        _context.Entry(existingVisit).CurrentValues.SetValues(visit);

        if (saveChanges)
        {
            await _context.SaveChangesAsync(ct);

            await _cache.RemoveByTagAsync(CacheTags.Visits, ct);
        }

        return visit;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid visitId, CancellationToken ct = default)
    {
        var visit = await _context.Visits.FindAsync([visitId], ct);
        if (visit == null)
            return false;

        _context.Visits.Remove(visit);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Visits, ct);

        return true;
    }
}
