namespace Billing.TimeCafe.Domain.Contracts;

public interface ITransactionRepository : IRepository<Transaction, Guid>
{
    Task<List<Transaction>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetBySourceAsync(TransactionSource source, Guid sourceId, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySourceAsync(TransactionSource source, Guid sourceId, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(List<Transaction> Items, int TotalCount)> GetPageAsync(int page, int pageSize, Guid? userId, CancellationToken cancellationToken = default);
}
