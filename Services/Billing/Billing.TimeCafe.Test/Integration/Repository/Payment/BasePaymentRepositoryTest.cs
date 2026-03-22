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

    protected async Task<PaymentModel> CreateTestPaymentAsync(
        Guid? paymentId = null,
        Guid? userId = null,
        decimal? amount = null,
        PaymentStatus status = PaymentStatus.Pending,
        string? externalPaymentId = null,
        DateTimeOffset? createdAt = null,
        CancellationToken ct = default)
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var payment = new PaymentModel(paymentId ?? DefaultsGuid.PaymentId)
        {
            UserId = userId ?? DefaultsGuid.UserId,
            Amount = amount ?? DefaultsGuid.DefaultAmount,
            PaymentMethod = PaymentMethod.Online,
            ExternalPaymentId = externalPaymentId,
            Status = status,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow
        };

        return await repository.CreateAsync(payment, ct);
    }

    protected async Task ClearCacheAsync(CancellationToken ct = default)
    {
        using var scope = CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        var hybridCache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var payments = db.Payments.ToList();
        foreach (var payment in payments)
        {
            var cacheKey = CacheKeys.Payment_ById(payment.PaymentId);
            await cache.RemoveAsync(cacheKey, ct);
        }

        await cache.RemoveAsync(CacheKeys.Payment_All, ct);
        await cache.RemoveAsync(CacheKeys.Payment_Pending, ct);
        await cache.RemoveAsync(CacheKeys.Payment_ByUserId(DefaultsGuid.UserId), ct);
        await cache.RemoveAsync(CacheKeys.Payment_ByUserId(DefaultsGuid.UserId2), ct);
        await cache.RemoveAsync(CacheKeys.Payment_ByUserId(DefaultsGuid.UserId3), ct);

        await hybridCache.RemoveByTagAsync(CacheTags.Payments, ct);
        await hybridCache.RemoveByTagAsync(CacheTags.PaymentByUser(DefaultsGuid.UserId), ct);
        await hybridCache.RemoveByTagAsync(CacheTags.PaymentByUser(DefaultsGuid.UserId2), ct);
        await hybridCache.RemoveByTagAsync(CacheTags.PaymentByUser(DefaultsGuid.UserId3), ct);

        foreach (var payment in payments)
        {
            await hybridCache.RemoveByTagAsync(CacheTags.PaymentByUser(payment.UserId), ct);
        }
    }

    protected async Task ClearDatabaseAsync(CancellationToken ct = default)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Payments.RemoveRange(db.Payments);
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
