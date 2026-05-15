namespace Billing.TimeCafe.Infrastructure.Repositories;

public sealed class TransactionRepository(
    ApplicationDbContext context,
    HybridCache cache) : ITransactionRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Transaction_ById(id),
            async token => await _context.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransactionId == id, token),
            tags: [CacheTags.Transactions, CacheTags.Transaction(id)],
            cancellationToken: cancellationToken);
    }

    public async Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Add(transaction);
        return await Task.FromResult(transaction);
    }

    public async Task<List<Transaction>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Transaction_History(userId, page, pageSize),
            async token => await _context.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(token),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Transactions, CacheTags.TransactionByUser(userId)],
            cancellationToken: cancellationToken);
    }

    public async Task<List<Transaction>> GetBySourceAsync(TransactionSource source, Guid sourceId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.Source == source && t.SourceId == sourceId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySourceAsync(TransactionSource source, Guid sourceId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AnyAsync(t => t.Source == source && t.SourceId == sourceId, cancellationToken);
    }

    public async Task<int> GetTotalCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .CountAsync(cancellationToken);
    }

    public async Task<(List<Transaction> Items, int TotalCount)> GetPageAsync(int page, int pageSize, Guid? userId, CancellationToken cancellationToken = default)
    {
        var tags = new List<string> { CacheTags.Transactions };
        if (userId.HasValue)
            tags.Add(CacheTags.TransactionByUser(userId.Value));

        return await _cache.GetOrCreateAsync(
            CacheKeys.Transaction_Page(page, pageSize, userId),
            async token =>
            {
                var query = _context.Transactions.AsNoTracking();
                if (userId.HasValue)
                    query = query.Where(t => t.UserId == userId.Value);
                query = query.OrderByDescending(t => t.CreatedAt);
                var totalCount = await query.CountAsync(token);
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(token);
                return (Items: items, TotalCount: totalCount);
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(2) },
            tags: tags,
            cancellationToken: cancellationToken);
    }

    public async Task<Transaction?> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Update(transaction);
        return await Task.FromResult(transaction);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionId == id, cancellationToken);
        if (transaction is not null)
        {
            _context.Transactions.Remove(transaction);
            return true;
        }
        return false;
    }
}
