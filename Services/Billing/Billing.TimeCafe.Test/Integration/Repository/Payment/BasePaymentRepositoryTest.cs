namespace Billing.TimeCafe.Test.Integration.Repository.Payment;

public abstract class BasePaymentRepositoryTest : IDisposable
{
    protected HttpClient Client { get; }
    protected IntegrationApiFactory Factory { get; }

    protected BasePaymentRepositoryTest()
    {
        Factory = new IntegrationApiFactory();
        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    protected async Task SaveAndInvalidateCacheAsync(IServiceScope scope, Guid paymentId, Guid userId)
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.SaveChangesAsync();
        var cache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        await cache.RemoveByTagAsync(CacheTags.Payments);
        await cache.RemoveByTagAsync(CacheTags.Payment(paymentId));
        await cache.RemoveByTagAsync(CacheTags.PaymentByUser(userId));
    }

    protected async Task<PaymentModel> CreateTestPaymentAsync(
        Guid? paymentId = null,
        Guid? userId = null,
        decimal? amount = null,
        PaymentStatus status = PaymentStatus.Pending,
        string? externalPaymentId = null,
        DateTimeOffset? createdAt = null,
        CancellationToken cancellationToken = default)
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var payment = new PaymentModel(paymentId ?? DefaultsGuid.PaymentId)
        {
            UserId = userId ?? DefaultsGuid.UserId,
            Amount = amount ?? DefaultsGuid.DefaultAmount,
            PaymentMethod = PaymentMethod.Online,
            ExternalPaymentId = externalPaymentId,
            Status = status,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow
        };

        var created = await repository.CreateAsync(payment, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return created;
    }

    protected async Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        using var scope = CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        var hybridCache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var payments = db.Payments.ToList();
        foreach (var payment in payments)
        {
            var cacheKey = CacheKeys.Payment_ById(payment.PaymentId);
            await cache.RemoveAsync(cacheKey, cancellationToken);
        }

        await cache.RemoveAsync(CacheKeys.Payment_All, cancellationToken);
        await cache.RemoveAsync(CacheKeys.Payment_Pending, cancellationToken);
        await cache.RemoveAsync(CacheKeys.Payment_ByUserId(DefaultsGuid.UserId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.Payment_ByUserId(DefaultsGuid.UserId2), cancellationToken);
        await cache.RemoveAsync(CacheKeys.Payment_ByUserId(DefaultsGuid.UserId3), cancellationToken);

        await hybridCache.RemoveByTagAsync(CacheTags.Payments, cancellationToken);
        await hybridCache.RemoveByTagAsync(CacheTags.PaymentByUser(DefaultsGuid.UserId), cancellationToken);
        await hybridCache.RemoveByTagAsync(CacheTags.PaymentByUser(DefaultsGuid.UserId2), cancellationToken);
        await hybridCache.RemoveByTagAsync(CacheTags.PaymentByUser(DefaultsGuid.UserId3), cancellationToken);

        foreach (var payment in payments)
        {
            await hybridCache.RemoveByTagAsync(CacheTags.PaymentByUser(payment.UserId), cancellationToken);
        }
    }

    protected async Task ClearDatabaseAsync(CancellationToken cancellationToken = default)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Payments.RemoveRange(db.Payments);
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
