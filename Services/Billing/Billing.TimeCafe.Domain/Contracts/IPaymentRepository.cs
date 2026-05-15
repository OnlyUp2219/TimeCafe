namespace Billing.TimeCafe.Domain.Contracts;

public interface IPaymentRepository : IRepository<Payment, Guid>
{
    Task<Payment?> GetByExternalPaymentIdAsync(string externalPaymentId, CancellationToken cancellationToken = default);
    Task<List<Payment>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(List<Payment> Items, int TotalCount)> GetPageAsync(int page, int pageSize, Guid? userId, CancellationToken cancellationToken = default);
}
