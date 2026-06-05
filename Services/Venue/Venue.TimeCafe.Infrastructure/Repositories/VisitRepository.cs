namespace Venue.TimeCafe.Infrastructure.Repositories;

public class VisitRepository(
    ApplicationDbContext context,
    HybridCache cache) : IVisitRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Visit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Visits.AsNoTracking().FirstOrDefaultAsync(v => v.VisitId == id, cancellationToken);

    public async Task<VisitWithTariffDto?> GetWithTariffByIdAsync(Guid visitId, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync(
            CacheKeys.Visit_ById(visitId),
            async cancellationToken => await (from v in _context.Visits
                               join t in _context.Tariffs on v.TariffId equals t.TariffId into tGroup
                               from t in tGroup.DefaultIfEmpty()
                               join r in _context.Resources on v.ResourceId equals r.ResourceId into rGroup
                               from r in rGroup.DefaultIfEmpty()
                               where v.VisitId == visitId
                               select new VisitWithTariffDto
                               {
                                   VisitId = v.VisitId,
                                   UserId = v.UserId,
                                   TariffId = v.TariffId,
                                   ResourceId = v.ResourceId,
                                   ResourceMaxGuests = r != null ? (int?)r.Capacity : null,
                                   EntryTime = v.EntryTime,
                                   ExitTime = v.ExitTime,
                                   CalculatedCost = v.CalculatedCost,
                                   Status = v.Status,
                                   TariffName = t != null ? t.Name : string.Empty,
                                   TariffPricePerMinute = t != null ? t.PricePerMinute : 0m,
                                   TariffDescription = t != null ? t.Description! : string.Empty,
                                   TariffBillingType = t != null ? t.BillingType : BillingType.PerMinute,
                                   TariffMinSessionMinutes = t != null ? t.MinSessionMinutes : null,
                                   TariffRoundingRule = t != null ? t.RoundingRule : null,
                                   PlannedMinutes = v.PlannedMinutes,
                                   GuestsCount = v.GuestsCount
                               })
                              .AsNoTracking()
                              .FirstOrDefaultAsync(cancellationToken),
            tags: [CacheTags.Visits, CacheTags.Visit(visitId)],
            cancellationToken: cancellationToken);

    public async Task<VisitWithTariffDto?> GetActiveVisitByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync(
            CacheKeys.Visit_ActiveByUser(userId),
            async cancellationToken => await (from v in _context.Visits
                               join t in _context.Tariffs on v.TariffId equals t.TariffId into tGroup
                               from t in tGroup.DefaultIfEmpty()
                               join r in _context.Resources on v.ResourceId equals r.ResourceId into rGroup
                               from r in rGroup.DefaultIfEmpty()
                               where v.UserId == userId && (v.Status == VisitStatus.Pending || v.Status == VisitStatus.Approved || v.Status == VisitStatus.Active || v.Status == VisitStatus.WaitingForPayment)
                               select new VisitWithTariffDto
                               {
                                   VisitId = v.VisitId,
                                   UserId = v.UserId,
                                   TariffId = v.TariffId,
                                   ResourceId = v.ResourceId,
                                   ResourceMaxGuests = r != null ? (int?)r.Capacity : null,
                                   EntryTime = v.EntryTime,
                                   ExitTime = v.ExitTime,
                                   CalculatedCost = v.CalculatedCost,
                                   Status = v.Status,
                                   ApprovedByUserId = v.ApprovedByUserId,
                                   ApprovedAt = v.ApprovedAt,
                                   RejectionReason = v.RejectionReason,
                                   TariffName = t != null ? t.Name : string.Empty,
                                   TariffPricePerMinute = t != null ? t.PricePerMinute : 0m,
                                   TariffDescription = t != null ? t.Description! : string.Empty,
                                   TariffBillingType = t != null ? t.BillingType : BillingType.PerMinute,
                                   TariffMinSessionMinutes = t != null ? t.MinSessionMinutes : null,
                                   TariffRoundingRule = t != null ? t.RoundingRule : null,
                                   PlannedMinutes = v.PlannedMinutes,
                                   GuestsCount = v.GuestsCount
                               })
                              .AsNoTracking()
                              .FirstOrDefaultAsync(cancellationToken),
            tags: [CacheTags.Visits, CacheTags.VisitByUser(userId)],
            cancellationToken: cancellationToken);

    public async Task<IEnumerable<VisitWithTariffDto>> GetActiveVisitsAsync(CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<VisitWithTariffDto>>(
            CacheKeys.Visit_Active,
            async cancellationToken => await (from v in _context.Visits
                               join t in _context.Tariffs on v.TariffId equals t.TariffId into tGroup
                               from t in tGroup.DefaultIfEmpty()
                               join r in _context.Resources on v.ResourceId equals r.ResourceId into rGroup
                               from r in rGroup.DefaultIfEmpty()
                               where v.Status == VisitStatus.Active
                               orderby v.EntryTime descending
                               select new VisitWithTariffDto
                               {
                                   VisitId = v.VisitId,
                                   UserId = v.UserId,
                                   TariffId = v.TariffId,
                                   ResourceId = v.ResourceId,
                                   ResourceMaxGuests = r != null ? (int?)r.Capacity : null,
                                   EntryTime = v.EntryTime,
                                   ExitTime = v.ExitTime,
                                   CalculatedCost = v.CalculatedCost,
                                   Status = v.Status,
                                   TariffName = t != null ? t.Name : string.Empty,
                                   TariffPricePerMinute = t != null ? t.PricePerMinute : 0m,
                                   TariffDescription = t != null ? t.Description! : string.Empty,
                                   TariffBillingType = t != null ? t.BillingType : BillingType.PerMinute,
                                   TariffMinSessionMinutes = t != null ? t.MinSessionMinutes : null,
                                   TariffRoundingRule = t != null ? t.RoundingRule : null,
                                   PlannedMinutes = v.PlannedMinutes,
                                   GuestsCount = v.GuestsCount
                               })
                              .AsNoTracking()
                              .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
            tags: [CacheTags.Visits],
            cancellationToken: cancellationToken) ?? [];

    public async Task<IEnumerable<VisitWithTariffDto>> GetVisitHistoryByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<VisitWithTariffDto>>(
            CacheKeys.Visit_HistoryByUser(userId, pageNumber, pageSize),
            async cancellationToken => await (from v in _context.Visits
                               join t in _context.Tariffs on v.TariffId equals t.TariffId into tGroup
                               from t in tGroup.DefaultIfEmpty()
                               join r in _context.Resources on v.ResourceId equals r.ResourceId into rGroup
                               from r in rGroup.DefaultIfEmpty()
                               where v.UserId == userId
                               orderby v.EntryTime descending
                               select new VisitWithTariffDto
                               {
                                   VisitId = v.VisitId,
                                   UserId = v.UserId,
                                   TariffId = v.TariffId,
                                   ResourceId = v.ResourceId,
                                   ResourceMaxGuests = r != null ? (int?)r.Capacity : null,
                                   EntryTime = v.EntryTime,
                                   ExitTime = v.ExitTime,
                                   CalculatedCost = v.CalculatedCost,
                                   Status = v.Status,
                                   TariffName = t != null ? t.Name : string.Empty,
                                   TariffPricePerMinute = t != null ? t.PricePerMinute : 0m,
                                   TariffDescription = t != null ? t.Description! : string.Empty,
                                   TariffBillingType = t != null ? t.BillingType : BillingType.PerMinute,
                                   TariffMinSessionMinutes = t != null ? t.MinSessionMinutes : null,
                                   TariffRoundingRule = t != null ? t.RoundingRule : null,
                                   PlannedMinutes = v.PlannedMinutes,
                                   GuestsCount = v.GuestsCount
                               })
                              .AsNoTracking()
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(10) },
            tags: [CacheTags.Visits, CacheTags.VisitByUser(userId)],
            cancellationToken: cancellationToken) ?? [];

    public async Task<IEnumerable<VisitWithTariffDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<VisitWithTariffDto>>(
            CacheKeys.Visit_Page(pageNumber, pageSize),
            async cancellationToken => await (from v in _context.Visits
                               join t in _context.Tariffs on v.TariffId equals t.TariffId into tGroup
                               from t in tGroup.DefaultIfEmpty()
                               join r in _context.Resources on v.ResourceId equals r.ResourceId into rGroup
                               from r in rGroup.DefaultIfEmpty()
                               orderby v.EntryTime descending
                               select new VisitWithTariffDto
                               {
                                   VisitId = v.VisitId,
                                   UserId = v.UserId,
                                   TariffId = v.TariffId,
                                   ResourceId = v.ResourceId,
                                   ResourceMaxGuests = r != null ? (int?)r.Capacity : null,
                                   EntryTime = v.EntryTime,
                                   ExitTime = v.ExitTime,
                                   CalculatedCost = v.CalculatedCost,
                                   Status = v.Status,
                                   TariffName = t != null ? t.Name : string.Empty,
                                   TariffPricePerMinute = t != null ? t.PricePerMinute : 0m,
                                   TariffDescription = t != null ? t.Description! : string.Empty,
                                   TariffBillingType = t != null ? t.BillingType : BillingType.PerMinute,
                                   TariffMinSessionMinutes = t != null ? t.MinSessionMinutes : null,
                                   TariffRoundingRule = t != null ? t.RoundingRule : null,
                                   PlannedMinutes = v.PlannedMinutes,
                                   GuestsCount = v.GuestsCount
                               })
                              .AsNoTracking()
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Visits],
            cancellationToken: cancellationToken) ?? [];

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default) =>
        await _context.Visits.CountAsync(cancellationToken);

    public async Task<bool> HasActiveVisitAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _context.Visits
            .AnyAsync(v => v.UserId == userId && (
                v.Status == VisitStatus.Pending ||
                v.Status == VisitStatus.Approved ||
                v.Status == VisitStatus.Active ||
                v.Status == VisitStatus.WaitingForPayment
            ), cancellationToken);

    public async Task<Visit> CreateAsync(Visit visit, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(visit);
        visit.EntryTime = DateTimeOffset.UtcNow;
        _context.Visits.Add(visit);
        return visit;
    }

    public async Task<IEnumerable<VisitWithTariffDto>> GetPendingVisitsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default) =>
        await _cache.GetOrCreateAsync<List<VisitWithTariffDto>>(
            CacheKeys.Visit_Pending(pageNumber, pageSize),
            async cancellationToken => await (from v in _context.Visits
                               join t in _context.Tariffs on v.TariffId equals t.TariffId into tGroup
                               from t in tGroup.DefaultIfEmpty()
                               join r in _context.Resources on v.ResourceId equals r.ResourceId into rGroup
                               from r in rGroup.DefaultIfEmpty()
                               where v.Status == VisitStatus.Pending
                               orderby v.EntryTime descending
                               select new VisitWithTariffDto
                               {
                                   VisitId = v.VisitId,
                                   UserId = v.UserId,
                                   TariffId = v.TariffId,
                                   ResourceId = v.ResourceId,
                                   ResourceMaxGuests = r != null ? (int?)r.Capacity : null,
                                   EntryTime = v.EntryTime,
                                   ExitTime = v.ExitTime,
                                   CalculatedCost = v.CalculatedCost,
                                   Status = v.Status,
                                   ApprovedByUserId = v.ApprovedByUserId,
                                   ApprovedAt = v.ApprovedAt,
                                   RejectionReason = v.RejectionReason,
                                   TariffName = t != null ? t.Name : string.Empty,
                                   TariffPricePerMinute = t != null ? t.PricePerMinute : 0m,
                                   TariffDescription = t != null ? t.Description! : string.Empty,
                                   TariffBillingType = t != null ? t.BillingType : BillingType.PerMinute,
                                   TariffMinSessionMinutes = t != null ? t.MinSessionMinutes : null,
                                   TariffRoundingRule = t != null ? t.RoundingRule : null,
                                   PlannedMinutes = v.PlannedMinutes,
                                   GuestsCount = v.GuestsCount
                               })
                              .AsNoTracking()
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync(cancellationToken),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
            tags: [CacheTags.Visits],
            cancellationToken: cancellationToken) ?? [];

    public async Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default) =>
        await _context.Visits.CountAsync(v => v.Status == VisitStatus.Pending, cancellationToken);

    public async Task<Visit?> UpdateAsync(Visit visit, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(visit);
        var existingVisit = await _context.Visits.FindAsync([visit.VisitId], cancellationToken);
        if (existingVisit == null)
            return null;

        _context.Entry(existingVisit).CurrentValues.SetValues(visit);

        return visit;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var visit = await _context.Visits.FindAsync([id], cancellationToken);
        if (visit == null) return false;

        _context.Visits.Remove(visit);
        return true;
    }

    public async Task<bool> IsResourceBusyAsync(Guid resourceId, CancellationToken cancellationToken = default) =>
        await IsResourceBusyAsync(resourceId, null, cancellationToken);

    public async Task<bool> IsResourceBusyAsync(Guid resourceId, Guid? excludeVisitId, CancellationToken cancellationToken = default) =>
        await _context.Visits.AnyAsync(v => v.ResourceId == resourceId && (excludeVisitId == null || v.VisitId != excludeVisitId) && (v.Status == VisitStatus.Active 
        || v.Status == VisitStatus.Pending 
        || v.Status == VisitStatus.Approved), cancellationToken);

    public async Task<bool> AnyWithTariffIdAsync(Guid tariffId, CancellationToken cancellationToken = default) =>
        await _context.Visits.AnyAsync(v => v.TariffId == tariffId, cancellationToken);
}

