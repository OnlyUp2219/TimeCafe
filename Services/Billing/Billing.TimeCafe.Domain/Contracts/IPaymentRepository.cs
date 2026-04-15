namespace Billing.TimeCafe.Domain.Contracts;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken ct = default);
    Task<Payment?> GetByExternalPaymentIdAsync(string externalPaymentId, CancellationToken ct = default);
    Task<Payment> CreateAsync(Payment payment, CancellationToken ct = default);
    Task<Payment> UpdateAsync(Payment payment, CancellationToken ct = default);
    Task<List<Payment>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<int> GetTotalCountByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<(List<Payment> Items, int TotalCount)> GetPageAsync(int page, int pageSize, Guid? userId, CancellationToken ct = default);
}
