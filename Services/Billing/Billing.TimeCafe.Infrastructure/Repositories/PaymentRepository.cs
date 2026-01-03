namespace Billing.TimeCafe.Infrastructure.Repositories;

public class PaymentRepository(
    ApplicationDbContext context,
    IDistributedCache cache,
    ILogger<PaymentRepository> cacheLogger) : IPaymentRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger _cacheLogger = cacheLogger;

    public async Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken ct = default)
    {
        var cached = await CacheHelper.GetAsync<Payment>(
            _cache,
            _cacheLogger,
            CacheKeys.Payment_ById(paymentId)).ConfigureAwait(false);
        if (cached != null)
            return cached;

        var payment = await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId, ct)
            .ConfigureAwait(false);

        if (payment != null)
        {
            await CacheHelper.SetAsync(
                _cache,
                _cacheLogger,
                CacheKeys.Payment_ById(paymentId),
                payment).ConfigureAwait(false);
        }

        return payment;
    }

    public async Task<Payment?> GetByExternalPaymentIdAsync(string externalPaymentId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(externalPaymentId))
            return null;

        var payment = await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId, ct)
            .ConfigureAwait(false);

        return payment;
    }

    public async Task<Payment> CreateAsync(Payment payment, CancellationToken ct = default)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(ct).ConfigureAwait(false);

        await CacheHelper.SetAsync(
            _cache,
            _cacheLogger,
            CacheKeys.Payment_ById(payment.PaymentId),
            payment).ConfigureAwait(false);

        return payment;
    }

    public async Task<Payment> UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(ct).ConfigureAwait(false);

        await _cache.RemoveAsync(CacheKeys.Payment_ById(payment.PaymentId), ct).ConfigureAwait(false);

        return payment;
    }

    public async Task<List<Payment>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var offset = (page - 1) * pageSize;

        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return payments;
    }

    public async Task<int> GetTotalCountByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var count = await _context.Payments
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .CountAsync(ct)
            .ConfigureAwait(false);

        return count;
    }
}
