namespace Billing.TimeCafe.Infrastructure.Repositories;

public sealed class PaymentRepository(
    ApplicationDbContext context,
    HybridCache cache) : IPaymentRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Payment_ById(id),
            async token => await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentId == id, token),
            tags: [CacheTags.Payments, CacheTags.Payment(id)],
            cancellationToken: cancellationToken);
    }

    public async Task<Payment?> GetByExternalPaymentIdAsync(string externalPaymentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalPaymentId))
            return null;

        return await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId, cancellationToken);
    }

    public async Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Add(payment);
        return await Task.FromResult(payment);
    }

    public async Task<Payment?> UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment);
        var existingPayment = await _context.Payments.FindAsync([payment.PaymentId], cancellationToken);
        if (existingPayment == null)
            return null;

        _context.Entry(existingPayment).CurrentValues.SetValues(payment);

        return existingPayment;
    }

    public async Task<List<Payment>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Payment_Page(page, pageSize, userId),
            async token => await _context.Payments
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(token),
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: [CacheTags.Payments, CacheTags.PaymentByUser(userId)],
            cancellationToken: cancellationToken);
    }

    public async Task<int> GetTotalCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .CountAsync(cancellationToken);
    }

    public async Task<(List<Payment> Items, int TotalCount)> GetPageAsync(int page, int pageSize, Guid? userId, CancellationToken cancellationToken = default)
    {
        var tags = new List<string> { CacheTags.Payments };
        if (userId.HasValue)
            tags.Add(CacheTags.PaymentByUser(userId.Value));

        return await _cache.GetOrCreateAsync(
            CacheKeys.Payment_Page(page, pageSize, userId),
            async token =>
            {
                var query = _context.Payments.AsNoTracking();
                if (userId.HasValue)
                    query = query.Where(p => p.UserId == userId.Value);
                query = query.OrderByDescending(p => p.CreatedAt);
                var totalCount = await query.CountAsync(token);
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(token);
                return (Items: items, TotalCount: totalCount);
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: tags,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == id, cancellationToken);
        if (payment is not null)
        {
            _context.Payments.Remove(payment);
            return true;
        }
        return false;
    }
}
