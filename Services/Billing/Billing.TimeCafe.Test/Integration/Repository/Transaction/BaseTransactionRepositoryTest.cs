namespace Billing.TimeCafe.Test.Integration.Repository.Transaction;

public abstract class BaseTransactionRepositoryTest : IDisposable
{
    protected HttpClient Client { get; }
    protected IntegrationApiFactory Factory { get; }

    protected BaseTransactionRepositoryTest()
    {
        Factory = new IntegrationApiFactory();
        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    protected async Task<TransactionModel> CreateTestTransactionAsync(
        Guid? userId = null,
        decimal? amount = null,
        TransactionType type = TransactionType.Deposit,
        TransactionSource source = TransactionSource.Manual,
        Guid? sourceId = null,
        CancellationToken ct = default)
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var transaction = type switch
        {
            TransactionType.Deposit => TransactionModel.CreateDeposit(
                userId ?? DefaultsGuid.UserId,
                amount ?? DefaultsGuid.DefaultAmount,
                source,
                sourceId),
            TransactionType.Withdrawal => TransactionModel.CreateWithdrawal(
                userId ?? DefaultsGuid.UserId,
                amount ?? DefaultsGuid.SmallAmount,
                source,
                sourceId),
            _ => throw new ArgumentException($"Unsupported type: {type}")
        };

        return await repository.CreateAsync(transaction, ct);
    }

    protected async Task ClearCacheAsync(CancellationToken ct = default)
    {
        using var scope = CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        var hybridCache = scope.ServiceProvider.GetRequiredService<HybridCache>();

        await cache.RemoveAsync(CacheKeys.Transaction_All, ct);
        await cache.RemoveAsync(CacheKeys.Transaction_ByUserId(DefaultsGuid.UserId), ct);
        await cache.RemoveAsync(CacheKeys.Transaction_ByUserId(DefaultsGuid.UserId2), ct);
        await cache.RemoveAsync(CacheKeys.Transaction_ByUserId(DefaultsGuid.UserId3), ct);

        await hybridCache.RemoveByTagAsync(CacheTags.Transactions, ct);
        await hybridCache.RemoveByTagAsync(CacheTags.TransactionByUser(DefaultsGuid.UserId), ct);
        await hybridCache.RemoveByTagAsync(CacheTags.TransactionByUser(DefaultsGuid.UserId2), ct);
        await hybridCache.RemoveByTagAsync(CacheTags.TransactionByUser(DefaultsGuid.UserId3), ct);
    }

    protected async Task ClearDatabaseAsync(CancellationToken ct = default)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Transactions.RemoveRange(db.Transactions);
        await db.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        try
        {
            Client?.Dispose();
        }
        catch
        {
        }

        try
        {
            Factory?.Dispose();
        }
        catch (NullReferenceException)
        {
        }

        GC.SuppressFinalize(this);
    }
}
