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

    protected async Task<BalanceModel> CreateTestBalanceAsync(Guid? userId = null, decimal? amount = null, CancellationToken ct = default)
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var balance = new BalanceModel(userId ?? Defaults.UserId)
        {
            CurrentBalance = amount ?? Defaults.DefaultAmount,
            TotalDeposited = amount ?? Defaults.DefaultAmount,
            TotalSpent = 0m,
            Debt = 0m
        };

        return await repository.CreateAsync(balance, ct);
    }

    protected async Task ClearCacheAsync(CancellationToken ct = default)
    {
        using var scope = CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var balances = db.Balances.ToList();
        foreach (var balance in balances)
        {
            var cacheKey = CacheKeys.Balance_ByUserId(balance.UserId);
            await cache.RemoveAsync(cacheKey, ct);
        }
        await cache.RemoveAsync(CacheKeys.Debtors_All, ct);
    }

    protected async Task ClearDatabaseAsync(CancellationToken ct = default)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Balances.RemoveRange(db.Balances);
        await db.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
