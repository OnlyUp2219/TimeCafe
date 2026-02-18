namespace Billing.TimeCafe.Infrastructure.Repositories;

public class TransactionRepository(
    ApplicationDbContext context,
    IDistributedCache cache,
    ILogger<TransactionRepository> cacheLogger) : ITransactionRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger _cacheLogger = cacheLogger;

    public async Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken ct = default)
    {
        var cached = await CacheHelper.GetAsync<Transaction>(
            _cache,
            _cacheLogger,
            CacheKeys.Transaction_ById(transactionId)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var transaction = await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId, ct)
            .ConfigureAwait(false);

        if (transaction != null)
        {
            await CacheHelper.SetAsync(
                _cache,
                _cacheLogger,
                CacheKeys.Transaction_ById(transactionId),
                transaction).ConfigureAwait(false);
        }

        return transaction;
    }

    public async Task<Transaction> CreateAsync(Transaction transaction, CancellationToken ct = default)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(ct).ConfigureAwait(false);

        await CacheHelper.RemoveKeysAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Transaction_All,
            CacheKeys.Transaction_ByUserId(transaction.UserId),
            CacheKeys.Transaction_History(transaction.UserId, 1),
            CacheKeys.Transaction_History(transaction.UserId, 2),
            CacheKeys.Transaction_History(transaction.UserId, 3),
            CacheKeys.Transaction_History(transaction.UserId, 4),
            CacheKeys.Transaction_History(transaction.UserId, 5)).ConfigureAwait(false);

        return transaction;
    }

    public async Task<List<Transaction>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var cacheKey = CacheKeys.Transaction_History(userId, page);
        var cached = await CacheHelper.GetAsync<List<Transaction>>(
            _cache,
            _cacheLogger,
            cacheKey).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            cacheKey,
            transactions,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) })
            .ConfigureAwait(false);

        return transactions;
    }

    public async Task<List<Transaction>> GetBySourceAsync(TransactionSource source, Guid sourceId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.Source == source && t.SourceId == sourceId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsBySourceAsync(TransactionSource source, Guid sourceId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .AnyAsync(t => t.Source == source && t.SourceId == sourceId, ct)
            .ConfigureAwait(false);
    }

    public async Task<int> GetTotalCountByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .CountAsync(ct)
            .ConfigureAwait(false);
    }
}
