namespace Billing.TimeCafe.Test.Integration.Repository.Payment;

public class UpdateAsyncTests : BasePaymentRepositoryTest
{
    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdatePaymentStatus_FromPendingToCompleted()
    {
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId, status: PaymentStatus.Pending);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var payment = await repository.GetByIdAsync(Defaults.PaymentId);
        payment.Should().NotBeNull();

        payment!.Status = PaymentStatus.Completed;

        await repository.UpdateAsync(payment);

        var retrieved = await repository.GetByIdAsync(Defaults.PaymentId);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_SetCompletedAtWhenStatusCompleted()
    {
        var completedAt = new DateTimeOffset(2025, 01, 01, 10, 05, 00, TimeSpan.Zero);
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId, status: PaymentStatus.Pending);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var payment = await repository.GetByIdAsync(Defaults.PaymentId);
        payment.Should().NotBeNull();

        payment!.Status = PaymentStatus.Completed;
        payment.CompletedAt = completedAt;

        await repository.UpdateAsync(payment);

        var retrieved = await repository.GetByIdAsync(Defaults.PaymentId);
        retrieved.Should().NotBeNull();
        retrieved!.CompletedAt.Should().Be(completedAt);
        retrieved.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_UpdateExternalPaymentId()
    {
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId, externalPaymentId: null);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var payment = await repository.GetByIdAsync(Defaults.PaymentId);
        payment.Should().NotBeNull();

        payment!.ExternalPaymentId = Defaults.StripePaymentIntentId;

        await repository.UpdateAsync(payment);

        var retrieved = await repository.GetByIdAsync(Defaults.PaymentId);
        retrieved.Should().NotBeNull();
        retrieved!.ExternalPaymentId.Should().Be(Defaults.StripePaymentIntentId);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_SetErrorMessageWhenFailed()
    {
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId, status: PaymentStatus.Pending);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var payment = await repository.GetByIdAsync(Defaults.PaymentId);
        payment.Should().NotBeNull();

        payment!.Status = PaymentStatus.Failed;
        payment.ErrorMessage = "Card declined";

        await repository.UpdateAsync(payment);

        var retrieved = await repository.GetByIdAsync(Defaults.PaymentId);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(PaymentStatus.Failed);
        retrieved.ErrorMessage.Should().Be("Card declined");
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_LinkTransactionId()
    {
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId, status: PaymentStatus.Pending);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var payment = await repository.GetByIdAsync(Defaults.PaymentId);
        payment.Should().NotBeNull();

        payment!.TransactionId = Defaults.TransactionId;

        await repository.UpdateAsync(payment);

        var retrieved = await repository.GetByIdAsync(Defaults.PaymentId);
        retrieved.Should().NotBeNull();
        retrieved!.TransactionId.Should().Be(Defaults.TransactionId);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_ReturnUpdatedPayment()
    {
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId, status: PaymentStatus.Pending);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var payment = await repository.GetByIdAsync(Defaults.PaymentId);
        payment.Should().NotBeNull();

        payment!.ExternalPaymentId = Defaults.StripePaymentIntentId;
        payment.Status = PaymentStatus.Pending;

        var updated = await repository.UpdateAsync(payment);

        updated.PaymentId.Should().Be(Defaults.PaymentId);
        updated.ExternalPaymentId.Should().Be(Defaults.StripePaymentIntentId);
    }

    [Fact]
    public async Task Repository_UpdateAsync_Should_InvalidateCache_ById()
    {
        await ClearCacheAsync();
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

        var cachedPayment = await repository.GetByIdAsync(Defaults.PaymentId);
        cachedPayment.Should().NotBeNull();

        var cacheKey = CacheKeys.Payment_ById(Defaults.PaymentId);
        var cachedBefore = await cache.GetStringAsync(cacheKey);
        cachedBefore.Should().NotBeNullOrEmpty();

        cachedPayment!.Status = PaymentStatus.Failed;
        cachedPayment.ErrorMessage = "Provider error";
        await repository.UpdateAsync(cachedPayment);

        var cachedAfter = await cache.GetStringAsync(cacheKey);
        cachedAfter.Should().BeNullOrEmpty();

        var retrieved = await repository.GetByIdAsync(Defaults.PaymentId);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(PaymentStatus.Failed);
    }
}
