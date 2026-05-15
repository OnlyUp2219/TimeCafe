namespace Billing.TimeCafe.Infrastructure.Data;

public class BillingTransactionExecutor(ApplicationDbContext db) : IBillingTransactionExecutor
{
    private readonly ApplicationDbContext _db = db;

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        if (string.Equals(_db.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.OrdinalIgnoreCase))
        {
            return await action(cancellationToken);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var result = await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async token =>
        {
            await action(token);
            return true;
        }, cancellationToken);
    }
}
