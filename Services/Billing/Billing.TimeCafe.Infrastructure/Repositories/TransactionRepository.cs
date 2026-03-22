namespace Billing.TimeCafe.Infrastructure.Repositories;

public class TransactionRepository(
    ApplicationDbContext context,
    HybridCache cache) : ITransactionRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Transaction_ById(transactionId),
            async token => await _context.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId, token),
            tags: [CacheTags.Transactions],
            cancellationToken: ct);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction, CancellationToken ct = default)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Transactions, ct);
        await _cache.RemoveByTagAsync(CacheTags.TransactionByUser(transaction.UserId), ct);

        return transaction;
    }

    public async Task<List<Transaction>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Transaction_History(userId, page),
            async token => await _context.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(token),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Transactions, CacheTags.TransactionByUser(userId)],
            cancellationToken: ct);
    }

    public async Task<List<Transaction>> GetBySourceAsync(TransactionSource source, Guid sourceId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.Source == source && t.SourceId == sourceId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsBySourceAsync(TransactionSource source, Guid sourceId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .AnyAsync(t => t.Source == source && t.SourceId == sourceId, ct);
    }

    public async Task<int> GetTotalCountByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .CountAsync(ct);
    }
}
