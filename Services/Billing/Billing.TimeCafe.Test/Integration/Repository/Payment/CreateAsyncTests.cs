namespace Billing.TimeCafe.Test.Integration.Repository.Payment;

public class CreateAsyncTests : BasePaymentRepositoryTest
{
    [Fact]
    public async Task Repository_CreateAsync_Should_InsertPayment_WhenValid()
    {
        var createdAt = new DateTimeOffset(2025, 01, 01, 10, 00, 00, TimeSpan.Zero);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var payment = new PaymentModel()
        {
            UserId = DefaultsGuid.UserId,
            Amount = DefaultsGuid.DefaultAmount,
            PaymentMethod = PaymentMethod.Online,
            CreatedAt = createdAt
        };

        var result = await repository.CreateAsync(payment);

        result.Should().NotBeNull();
        result.PaymentId.Should().NotBe(Guid.Empty);
        result.UserId.Should().Be(DefaultsGuid.UserId);
        result.Amount.Should().Be(DefaultsGuid.DefaultAmount);
        result.Status.Should().Be(PaymentStatus.Pending);
        result.CreatedAt.Should().Be(createdAt);

        var retrieved = await repository.GetByIdAsync(result.PaymentId);
        retrieved.Should().NotBeNull();
        retrieved!.PaymentId.Should().Be(result.PaymentId);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_SetDefaultPaymentStatus_AsPending()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var payment = new PaymentModel()
        {
            UserId = DefaultsGuid.UserId,
            Amount = DefaultsGuid.DefaultAmount,
            PaymentMethod = PaymentMethod.Online
        };

        var result = await repository.CreateAsync(payment);

        result.Status.Should().Be(PaymentStatus.Pending);

        var retrieved = await repository.GetByIdAsync(result.PaymentId);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_SetUniquePaymentId()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var payment1 = new PaymentModel()
        {
            UserId = DefaultsGuid.UserId,
            Amount = DefaultsGuid.DefaultAmount,
            PaymentMethod = PaymentMethod.Online
        };

        var payment2 = new PaymentModel()
        {
            UserId = DefaultsGuid.UserId,
            Amount = DefaultsGuid.SmallAmount,
            PaymentMethod = PaymentMethod.Online
        };

        var created1 = await repository.CreateAsync(payment1);
        var created2 = await repository.CreateAsync(payment2);

        created1.PaymentId.Should().NotBe(Guid.Empty);
        created2.PaymentId.Should().NotBe(Guid.Empty);
        created1.PaymentId.Should().NotBe(created2.PaymentId);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_ReturnCreatedPayment()
    {
        var createdAt = new DateTimeOffset(2025, 01, 01, 10, 00, 00, TimeSpan.Zero);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var payment = new PaymentModel()
        {
            UserId = DefaultsGuid.UserId,
            Amount = DefaultsGuid.DefaultAmount,
            PaymentMethod = PaymentMethod.Online,
            CreatedAt = createdAt
        };

        var result = await repository.CreateAsync(payment);

        result.PaymentId.Should().NotBe(Guid.Empty);
        result.UserId.Should().Be(DefaultsGuid.UserId);
        result.Amount.Should().Be(DefaultsGuid.DefaultAmount);
        result.PaymentMethod.Should().Be(PaymentMethod.Online);
        result.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task Repository_CreateAsync_Should_SetCache_ById()
    {
        await ClearCacheAsync();

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

        var payment = new PaymentModel(DefaultsGuid.PaymentId)
        {
            UserId = DefaultsGuid.UserId,
            Amount = DefaultsGuid.DefaultAmount,
            PaymentMethod = PaymentMethod.Online,
            Status = PaymentStatus.Pending,
            CreatedAt = new DateTimeOffset(2025, 01, 01, 10, 00, 00, TimeSpan.Zero)
        };

        await repository.CreateAsync(payment);

        var cacheKey = CacheKeys.Payment_ById(DefaultsGuid.PaymentId);
        var cachedValue = await cache.GetStringAsync(cacheKey);
        cachedValue.Should().NotBeNullOrEmpty();
    }
}
