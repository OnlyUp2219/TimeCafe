namespace Billing.TimeCafe.Infrastructure.Repositories;

public sealed class InvoiceRepository(
    ApplicationDbContext context,
    HybridCache cache) : IInvoiceRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Invoice_ById(id),
            async token => await _context.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceId == id, token),
            tags: [CacheTags.Invoices, CacheTags.Invoice(id)],
            cancellationToken: cancellationToken);
    }

    public async Task<Invoice?> GetByVisitIdAsync(Guid visitId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Invoice_ByVisitId(visitId),
            async token => await _context.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.VisitId == visitId, token),
            tags: [CacheTags.Invoices],
            cancellationToken: cancellationToken);
    }

    public async Task<Invoice?> GetByStripeSessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return null;

        return await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.StripeSessionId == sessionId, cancellationToken);
    }

    public async Task<Invoice> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _context.Invoices.Add(invoice);
        return await Task.FromResult(invoice);
    }

    public async Task<Invoice?> UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        var existing = await _context.Invoices.FindAsync([invoice.InvoiceId], cancellationToken);
        if (existing == null)
            return null;

        _context.Entry(existing).CurrentValues.SetValues(invoice);

        return existing;
    }

    public async Task<(List<Invoice> Items, int TotalCount)> GetPageAsync(int page, int pageSize, Guid? userId, CancellationToken cancellationToken = default)
    {
        var tags = new List<string> { CacheTags.Invoices };
        if (userId.HasValue)
            tags.Add(CacheTags.InvoiceByUser(userId.Value));

        return await _cache.GetOrCreateAsync(
            CacheKeys.Invoice_Page(page, pageSize, userId),
            async token =>
            {
                var query = _context.Invoices.AsNoTracking();
                if (userId.HasValue)
                    query = query.Where(i => i.UserId == userId.Value);
                
                query = query.OrderByDescending(i => i.CreatedAt);
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
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == id, cancellationToken);
        if (invoice is not null)
        {
            _context.Invoices.Remove(invoice);
            return true;
        }
        return false;
    }
}
