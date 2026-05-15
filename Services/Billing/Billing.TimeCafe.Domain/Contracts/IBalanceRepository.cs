namespace Billing.TimeCafe.Domain.Contracts;

public interface IBalanceRepository : IRepository<Balance, Guid>
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Balance>> GetUsersWithDebtAsync(CancellationToken cancellationToken = default);
    Task<(List<Balance> Items, int TotalCount)> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Balance>> GetByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}
