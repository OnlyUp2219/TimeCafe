namespace Billing.TimeCafe.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken ct = default);
    Task<Transaction> CreateAsync(Transaction transaction, CancellationToken ct = default);
    Task<List<Transaction>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<List<Transaction>> GetBySourceAsync(TransactionSource source, Guid sourceId, CancellationToken ct = default);
    Task<bool> ExistsBySourceAsync(TransactionSource source, Guid sourceId, CancellationToken ct = default);
    Task<int> GetTotalCountByUserIdAsync(Guid userId, CancellationToken ct = default);
}
