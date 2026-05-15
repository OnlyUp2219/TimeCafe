namespace Billing.TimeCafe.Domain.Contracts;

public interface IUnitOfWork
{
    IBalanceRepository Balances { get; }
    IPaymentRepository Payments { get; }
    ITransactionRepository Transactions { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
