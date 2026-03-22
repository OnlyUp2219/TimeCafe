namespace Billing.TimeCafe.Infrastructure.Repositories;

public class PaymentRepository(
    ApplicationDbContext context,
    HybridCache cache) : IPaymentRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Payment_ById(paymentId),
            async token => await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId, token),
            tags: [CacheTags.Payments],
            cancellationToken: ct);
    }

    public async Task<Payment?> GetByExternalPaymentIdAsync(string externalPaymentId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(externalPaymentId))
            return null;

        return await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId, ct);
    }

    public async Task<Payment> CreateAsync(Payment payment, CancellationToken ct = default)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Payments, ct);
        await _cache.RemoveByTagAsync(CacheTags.PaymentByUser(payment.UserId), ct);

        return payment;
    }

    public async Task<Payment> UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync(CacheTags.Payments, ct);
        await _cache.RemoveByTagAsync(CacheTags.PaymentByUser(payment.UserId), ct);

        return payment;
    }

    public async Task<List<Payment>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var offset = (page - 1) * pageSize;

        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalCountByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .CountAsync(ct);
    }
}
