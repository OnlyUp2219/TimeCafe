namespace Auth.TimeCafe.Infrastructure.Data;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => context.Database.BeginTransactionAsync(cancellationToken);

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => context.Database.CommitTransactionAsync(cancellationToken);

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => context.Database.RollbackTransactionAsync(cancellationToken);
}
