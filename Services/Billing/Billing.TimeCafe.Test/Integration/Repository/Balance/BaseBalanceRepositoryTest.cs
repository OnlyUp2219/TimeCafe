namespace Billing.TimeCafe.Test.Integration.Repository.Balance;

public abstract class BaseBalanceRepositoryTest : IDisposable
{
    protected HttpClient Client { get; }
    protected IntegrationApiFactory Factory { get; }

    protected BaseBalanceRepositoryTest()
    {
        Factory = new IntegrationApiFactory();
        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    protected async Task SaveAndInvalidateCacheAsync(IServiceScope scope, Guid userId)
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.SaveChangesAsync();
        var cache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        await cache.RemoveByTagAsync(CacheTags.Balances);
        await cache.RemoveByTagAsync(CacheTags.Balance(userId));
    }

    protected async Task<BalanceModel> CreateTestBalanceAsync(Guid? userId = null, decimal? amount = null, CancellationToken cancellationToken = default)
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var balance = new BalanceModel(userId ?? DefaultsGuid.UserId)
        {
            CurrentBalance = amount ?? DefaultsGuid.DefaultAmount,
            TotalDeposited = amount ?? DefaultsGuid.DefaultAmount,
            TotalSpent = 0m,
            Debt = 0m
        };

        var created = await repository.CreateAsync(balance, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return created;
    }

    protected async Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        using var scope = CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        var hybridCache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var balances = await db.Balances.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var balance in balances)
        {
            var cacheKey = CacheKeys.Balance_ByUserId(balance.UserId);
            await cache.RemoveAsync(cacheKey, cancellationToken);
        }
        await cache.RemoveAsync(CacheKeys.Debtors_All, cancellationToken);
        await cache.RemoveAsync(CacheKeys.Balance_All, cancellationToken);

        await hybridCache.RemoveByTagAsync(CacheTags.Balances, cancellationToken);
        foreach (var balance in balances)
        {
            await hybridCache.RemoveByTagAsync(CacheTags.Balance(balance.UserId), cancellationToken);
        }
    }

    protected async Task ClearDatabaseAsync(CancellationToken cancellationToken = default)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var balances = await db.Balances.ToListAsync(cancellationToken);
        db.Balances.RemoveRange(balances);
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
