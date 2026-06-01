namespace Billing.TimeCafe.Domain.Contracts;

public interface IInvoiceRepository : IRepository<Invoice, Guid>
{
    Task<Invoice?> GetByVisitIdAsync(Guid visitId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByStripeSessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<(List<Invoice> Items, int TotalCount)> GetPageAsync(int page, int pageSize, Guid? userId, CancellationToken cancellationToken = default);
}
