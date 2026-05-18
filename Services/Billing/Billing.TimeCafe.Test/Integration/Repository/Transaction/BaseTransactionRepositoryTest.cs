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

    protected async Task SaveAndInvalidateCacheAsync(IServiceScope scope, Guid transactionId, Guid userId)
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.SaveChangesAsync();
        var cache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        await cache.RemoveByTagAsync(CacheTags.Transactions);
        await cache.RemoveByTagAsync(CacheTags.Transaction(transactionId));
        await cache.RemoveByTagAsync(CacheTags.TransactionByUser(userId));
    }

    protected async Task<TransactionModel> CreateTestTransactionAsync(
        Guid? userId = null,
        decimal? amount = null,
        TransactionType type = TransactionType.Deposit,
        TransactionSource source = TransactionSource.Manual,
        Guid? sourceId = null,
        CancellationToken cancellationToken = default)
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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

        var created = await repository.CreateAsync(transaction, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return created;
    }

    protected async Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        using var scope = CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        var hybridCache = scope.ServiceProvider.GetRequiredService<HybridCache>();

        await cache.RemoveAsync(CacheKeys.Transaction_All, cancellationToken);
        await cache.RemoveAsync(CacheKeys.Transaction_ByUserId(DefaultsGuid.UserId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.Transaction_ByUserId(DefaultsGuid.UserId2), cancellationToken);
        await cache.RemoveAsync(CacheKeys.Transaction_ByUserId(DefaultsGuid.UserId3), cancellationToken);

        await hybridCache.RemoveByTagAsync(CacheTags.Transactions, cancellationToken);
        await hybridCache.RemoveByTagAsync(CacheTags.TransactionByUser(DefaultsGuid.UserId), cancellationToken);
        await hybridCache.RemoveByTagAsync(CacheTags.TransactionByUser(DefaultsGuid.UserId2), cancellationToken);
        await hybridCache.RemoveByTagAsync(CacheTags.TransactionByUser(DefaultsGuid.UserId3), cancellationToken);
    }

    protected async Task ClearDatabaseAsync(CancellationToken cancellationToken = default)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Transactions.RemoveRange(db.Transactions);
        await db.SaveChangesAsync(cancellationToken);
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
