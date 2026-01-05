namespace Billing.TimeCafe.Infrastructure.Repositories;

public class BalanceRepository(
    ApplicationDbContext context,
    IDistributedCache cache,
    ILogger<BalanceRepository> cacheLogger) : IBalanceRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger _cacheLogger = cacheLogger;

    public async Task<Balance?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var cached = await CacheHelper.GetAsync<Balance>(
            _cache,
            _cacheLogger,
            CacheKeys.Balance_ByUserId(userId)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var balance = await _context.Balances
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == userId, ct)
            .ConfigureAwait(false);

        if (balance != null)
        {
            await CacheHelper.SetAsync(
                _cache,
                _cacheLogger,
                CacheKeys.Balance_ByUserId(userId),
                balance).ConfigureAwait(false);
        }

        return balance;
    }

    public async Task<Balance> CreateAsync(Balance balance, CancellationToken ct = default)
    {
        var existingBalance = await GetByUserIdAsync(balance.UserId, ct).ConfigureAwait(false);
        if (existingBalance != null)
            return existingBalance;

        _context.Balances.Add(balance);

        try
        {
            await _context.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            var created = await GetByUserIdAsync(balance.UserId, ct).ConfigureAwait(false);
            if (created != null)
                return created;
            throw;
        }
        catch (ArgumentException)
        {
            var created = await GetByUserIdAsync(balance.UserId, ct).ConfigureAwait(false);
            if (created != null)
                return created;
            throw;
        }

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Balance_All,
            CacheKeys.Balance_ByUserId(balance.UserId),
            CacheKeys.Debtors_All).ConfigureAwait(false);

        return balance;
    }

    public async Task<Balance> UpdateAsync(Balance balance, CancellationToken ct = default)
    {
        _context.Balances.Update(balance);
        await _context.SaveChangesAsync(ct).ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Balance_All,
            CacheKeys.Balance_ByUserId(balance.UserId),
            CacheKeys.Debtors_All).ConfigureAwait(false);

        return balance;
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Balances
            .AnyAsync(b => b.UserId == userId, ct)
            .ConfigureAwait(false);
    }

    public async Task<List<Balance>> GetUsersWithDebtAsync(CancellationToken ct = default)
    {
        var cached = await CacheHelper.GetAsync<List<Balance>>(
            _cache,
            _cacheLogger,
            CacheKeys.Debtors_All).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var debtors = await _context.Balances
            .AsNoTracking()
            .Where(b => b.Debt > 0)
            .OrderByDescending(b => b.Debt)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Debtors_All,
            debtors,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) })
            .ConfigureAwait(false);

        return debtors;
    }
}
