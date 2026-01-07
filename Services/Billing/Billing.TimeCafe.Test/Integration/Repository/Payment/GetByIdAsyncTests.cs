namespace Billing.TimeCafe.Test.Integration.Repository.Payment;

public class GetByIdAsyncTests : BasePaymentRepositoryTest
{
    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnPayment_WhenExists()
    {
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByIdAsync(Defaults.PaymentId);

        result.Should().NotBeNull();
        result!.PaymentId.Should().Be(Defaults.PaymentId);
        result.UserId.Should().Be(Defaults.UserId);
        result.Amount.Should().Be(Defaults.DefaultAmount);
        result.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnNull_WhenNotExists()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByIdAsync(InvalidData.NonExistentPaymentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnFromCache_WhenCalledTwice()
    {
        await ClearCacheAsync();
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

        var result1 = await repository.GetByIdAsync(Defaults.PaymentId);

        var cacheKey = CacheKeys.Payment_ById(Defaults.PaymentId);
        var cachedValue = await cache.GetStringAsync(cacheKey);
        cachedValue.Should().NotBeNullOrEmpty();

        var result2 = await repository.GetByIdAsync(Defaults.PaymentId);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1!.PaymentId.Should().Be(result2!.PaymentId);
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnNull_WhenEmptyGuid()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByIdAsync(InvalidData.EmptyUserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByIdAsync_Should_ReturnPaymentWithAllFields()
    {
        var createdAt = new DateTimeOffset(2025, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var completedAt = new DateTimeOffset(2025, 01, 01, 10, 05, 00, TimeSpan.Zero);

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

            var payment = new PaymentModel(Defaults.PaymentId)
            {
                UserId = Defaults.UserId,
                Amount = Defaults.DefaultAmount,
                PaymentMethod = PaymentMethod.Online,
                ExternalPaymentId = Defaults.StripePaymentIntentId,
                Status = PaymentStatus.Completed,
                TransactionId = Defaults.TransactionId,
                ExternalData = "{\"k\":\"v\"}",
                CreatedAt = createdAt,
                CompletedAt = completedAt,
                ErrorMessage = null
            };

            await repository.CreateAsync(payment);
        }

        using var scope2 = CreateScope();
        var repository2 = scope2.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository2.GetByIdAsync(Defaults.PaymentId);

        result.Should().NotBeNull();
        result!.PaymentId.Should().Be(Defaults.PaymentId);
        result.UserId.Should().Be(Defaults.UserId);
        result.Amount.Should().Be(Defaults.DefaultAmount);
        result.PaymentMethod.Should().Be(PaymentMethod.Online);
        result.ExternalPaymentId.Should().Be(Defaults.StripePaymentIntentId);
        result.Status.Should().Be(PaymentStatus.Completed);
        result.TransactionId.Should().Be(Defaults.TransactionId);
        result.ExternalData.Should().Be("{\"k\":\"v\"}");
        result.CreatedAt.Should().Be(createdAt);
        result.CompletedAt.Should().Be(completedAt);
        result.ErrorMessage.Should().BeNull();
    }
}
