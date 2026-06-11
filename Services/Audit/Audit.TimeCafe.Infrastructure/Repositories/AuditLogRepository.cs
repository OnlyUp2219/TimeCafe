namespace Audit.TimeCafe.Infrastructure.Repositories;

public sealed class AuditLogRepository(ApplicationDbContext context, HybridCache cache) : IAuditLogRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.AuditLog_ById(id),
            async token => await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, token),
            tags: [CacheTags.AuditLogs, CacheTags.AuditLog(id)],
            cancellationToken: cancellationToken);
    }

    public async Task<AuditLog> CreateAsync(AuditLog entity, CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.Add(entity);
        return await Task.FromResult(entity);
    }

    public async Task<AuditLog?> UpdateAsync(AuditLog entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var existing = await _context.AuditLogs.FindAsync([entity.Id], cancellationToken);
        if (existing == null)
            return null;

        _context.Entry(existing).CurrentValues.SetValues(entity);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.AuditLogs.FindAsync([id], cancellationToken);
        if (entity == null)
            return false;

        _context.AuditLogs.Remove(entity);
        return true;
    }

    public async Task<(List<AuditLog> Items, int TotalCount)> GetPageAsync(
        int page, int pageSize, string? eventType, string? userName, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(a => a.EventType.Contains(eventType));

        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(a => a.UserName.Contains(userName));

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
