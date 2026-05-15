namespace Billing.TimeCafe.Infrastructure.Repositories;

public sealed class BalanceRepository(
    ApplicationDbContext context,
    HybridCache cache) : IBalanceRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Balance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Balance_ByUserId(id),
            async token => await _context.Balances
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.UserId == id, token),
            tags: [CacheTags.Balances, CacheTags.Balance(id)],
            cancellationToken: cancellationToken);
    }

    public async Task<Balance> CreateAsync(Balance balance, CancellationToken cancellationToken = default)
    {
        _context.Balances.Add(balance);
        return await Task.FromResult(balance);
    }

    public async Task<Balance> UpdateAsync(Balance balance, CancellationToken cancellationToken = default)
    {
        _context.Balances.Update(balance);
        return await Task.FromResult(balance);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Balances
            .AnyAsync(b => b.UserId == id, cancellationToken);
    }

    public async Task<List<Balance>> GetUsersWithDebtAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Debtors_All,
            async token => await _context.Balances
                .AsNoTracking()
                .Where(b => b.Debt > 0)
                .OrderByDescending(b => b.Debt)
                .ToListAsync(token),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(10) },
            tags: [CacheTags.Balances],
            cancellationToken: cancellationToken);
    }

    public async Task<(List<Balance> Items, int TotalCount)> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Balance_Page(page, pageSize),
            async token =>
            {
                var query = _context.Balances.AsNoTracking().OrderByDescending(b => b.LastUpdated);
                var totalCount = await query.CountAsync(token);
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(token);
                return (Items: items, TotalCount: totalCount);
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Balances],
            cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var balance = await _context.Balances.FirstOrDefaultAsync(b => b.UserId == id, cancellationToken);
        if (balance is not null)
        {
            _context.Balances.Remove(balance);
            return true;
        }
        return false;
    }

    public async Task<List<Balance>> GetByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        return await _context.Balances
            .AsNoTracking()
            .Where(b => userIds.Contains(b.UserId))
            .ToListAsync(cancellationToken);
    }
}
