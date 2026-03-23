namespace Billing.TimeCafe.Infrastructure.Repositories;

public class BalanceRepository(
    ApplicationDbContext context,
    HybridCache cache) : IBalanceRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Balance?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Balance_ByUserId(userId),
            async token => await _context.Balances
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.UserId == userId, token),
            tags: [CacheTags.Balances, CacheTags.Balance(userId)],
            cancellationToken: ct);
    }

    public async Task<Balance> CreateAsync(Balance balance, CancellationToken ct = default)
    {
        var existingBalance = await GetByUserIdAsync(balance.UserId, ct);
        if (existingBalance != null)
            return existingBalance;

        _context.Balances.Add(balance);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Balances, ct);

        return balance;
    }

    public async Task<Balance> UpdateAsync(Balance balance, CancellationToken ct = default)
    {
        _context.Balances.Update(balance);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Balances, ct);
        await _cache.RemoveByTagAsync(CacheTags.Transactions, ct);
        await _cache.RemoveByTagAsync(CacheTags.TransactionByUser(balance.UserId), ct);

        return balance;
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Balances
            .AnyAsync(b => b.UserId == userId, ct);
    }

    public async Task<List<Balance>> GetUsersWithDebtAsync(CancellationToken ct = default)
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
            cancellationToken: ct);
    }
}
