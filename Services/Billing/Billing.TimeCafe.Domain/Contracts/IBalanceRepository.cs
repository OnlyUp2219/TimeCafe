namespace Billing.TimeCafe.Domain.Contracts;

public interface IBalanceRepository
{
    Task<Balance?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Balance> CreateAsync(Balance balance, CancellationToken ct = default);
    Task<Balance> UpdateAsync(Balance balance, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default);
    Task<List<Balance>> GetUsersWithDebtAsync(CancellationToken ct = default);
}
