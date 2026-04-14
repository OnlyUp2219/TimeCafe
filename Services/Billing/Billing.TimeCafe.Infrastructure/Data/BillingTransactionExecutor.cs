namespace Billing.TimeCafe.Infrastructure.Data;

public class BillingTransactionExecutor(ApplicationDbContext db) : IBillingTransactionExecutor
{
    private readonly ApplicationDbContext _db = db;

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var result = await action(ct);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
    {
        await ExecuteAsync(async token =>
        {
            await action(token);
            return true;
        }, ct);
    }
}