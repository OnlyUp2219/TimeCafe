namespace Billing.TimeCafe.Test.Integration.Repository.Payment;

public class GetByUserIdAsyncTests : BasePaymentRepositoryTest
{
    [Fact]
    public async Task Repository_GetByUserIdAsync_Should_ReturnAllPaymentsByUser()
    {
        await CreateTestPaymentAsync(paymentId: DefaultsGuid.PaymentId, userId: DefaultsGuid.UserId);
        await CreateTestPaymentAsync(paymentId: DefaultsGuid.PaymentId2, userId: DefaultsGuid.UserId);
        await CreateTestPaymentAsync(paymentId: DefaultsGuid.PaymentId3, userId: DefaultsGuid.UserId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByUserIdAsync(DefaultsGuid.UserId, page: 1, pageSize: 10);

        result.Should().HaveCount(3);
        result.Select(x => x.PaymentId).Should().Contain(new[]
        {
            DefaultsGuid.PaymentId,
            DefaultsGuid.PaymentId2,
            DefaultsGuid.PaymentId3
        });
    }

    [Fact]
    public async Task Repository_GetByUserIdAsync_Should_NotReturnPaymentsFromOtherUsers()
    {
        await CreateTestPaymentAsync(paymentId: DefaultsGuid.PaymentId, userId: DefaultsGuid.UserId);
        await CreateTestPaymentAsync(paymentId: DefaultsGuid.PaymentId2, userId: DefaultsGuid.UserId2);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByUserIdAsync(DefaultsGuid.UserId, page: 1, pageSize: 10);

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(DefaultsGuid.UserId);
        result[0].PaymentId.Should().Be(DefaultsGuid.PaymentId);
    }

    [Fact]
    public async Task Repository_GetByUserIdAsync_Should_ReturnPaymentsOrderedByCreatedAtDesc_WithPaging()
    {
        var t1 = new DateTimeOffset(2025, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2025, 01, 01, 10, 01, 00, TimeSpan.Zero);
        var t3 = new DateTimeOffset(2025, 01, 01, 10, 02, 00, TimeSpan.Zero);

        await CreateTestPaymentAsync(paymentId: DefaultsGuid.PaymentId, userId: DefaultsGuid.UserId, createdAt: t1);
        await CreateTestPaymentAsync(paymentId: DefaultsGuid.PaymentId2, userId: DefaultsGuid.UserId, createdAt: t2);
        await CreateTestPaymentAsync(paymentId: DefaultsGuid.PaymentId3, userId: DefaultsGuid.UserId, createdAt: t3);

        await CreateTestPaymentAsync(paymentId: DefaultsGuid.PaymentId4, userId: DefaultsGuid.UserId2, createdAt: t3);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var page1 = await repository.GetByUserIdAsync(DefaultsGuid.UserId, page: 1, pageSize: 2);
        page1.Should().HaveCount(2);
        page1[0].PaymentId.Should().Be(DefaultsGuid.PaymentId3);
        page1[1].PaymentId.Should().Be(DefaultsGuid.PaymentId2);

        var page2 = await repository.GetByUserIdAsync(DefaultsGuid.UserId, page: 2, pageSize: 2);
        page2.Should().HaveCount(1);
        page2[0].PaymentId.Should().Be(DefaultsGuid.PaymentId);
    }

    [Fact]
    public async Task Repository_GetByUserIdAsync_Should_ReturnEmptyList_WhenUserHasNoPayments()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByUserIdAsync(InvalidDataGuid.NonExistentUserId, page: 1, pageSize: 10);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
