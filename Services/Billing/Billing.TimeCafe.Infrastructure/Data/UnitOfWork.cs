namespace Billing.TimeCafe.Infrastructure.Data;

public class UnitOfWork(ApplicationDbContext context, HybridCache cache) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public IBalanceRepository Balances => field ??= new BalanceRepository(_context, _cache);
    public IPaymentRepository Payments => field ??= new PaymentRepository(_context, _cache);
    public ITransactionRepository Transactions => field ??= new TransactionRepository(_context, _cache);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        await _context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
        await _context.Database.CommitTransactionAsync(cancellationToken);

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
        await _context.Database.RollbackTransactionAsync(cancellationToken);
}
