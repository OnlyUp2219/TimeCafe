namespace Billing.TimeCafe.Test.Integration.Repository.Payment;

public class GetByUserIdAsyncTests : BasePaymentRepositoryTest
{
    [Fact]
    public async Task Repository_GetByUserIdAsync_Should_ReturnAllPaymentsByUser()
    {
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId, userId: Defaults.UserId);
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId2, userId: Defaults.UserId);
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId3, userId: Defaults.UserId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByUserIdAsync(Defaults.UserId, page: 1, pageSize: 10);

        result.Should().HaveCount(3);
        result.Select(x => x.PaymentId).Should().Contain(new[]
        {
            Defaults.PaymentId,
            Defaults.PaymentId2,
            Defaults.PaymentId3
        });
    }

    [Fact]
    public async Task Repository_GetByUserIdAsync_Should_NotReturnPaymentsFromOtherUsers()
    {
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId, userId: Defaults.UserId);
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId2, userId: Defaults.UserId2);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByUserIdAsync(Defaults.UserId, page: 1, pageSize: 10);

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(Defaults.UserId);
        result[0].PaymentId.Should().Be(Defaults.PaymentId);
    }

    [Fact]
    public async Task Repository_GetByUserIdAsync_Should_ReturnPaymentsOrderedByCreatedAtDesc_WithPaging()
    {
        var t1 = new DateTimeOffset(2025, 01, 01, 10, 00, 00, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2025, 01, 01, 10, 01, 00, TimeSpan.Zero);
        var t3 = new DateTimeOffset(2025, 01, 01, 10, 02, 00, TimeSpan.Zero);

        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId, userId: Defaults.UserId, createdAt: t1);
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId2, userId: Defaults.UserId, createdAt: t2);
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId3, userId: Defaults.UserId, createdAt: t3);

        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId4, userId: Defaults.UserId2, createdAt: t3);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var page1 = await repository.GetByUserIdAsync(Defaults.UserId, page: 1, pageSize: 2);
        page1.Should().HaveCount(2);
        page1[0].PaymentId.Should().Be(Defaults.PaymentId3);
        page1[1].PaymentId.Should().Be(Defaults.PaymentId2);

        var page2 = await repository.GetByUserIdAsync(Defaults.UserId, page: 2, pageSize: 2);
        page2.Should().HaveCount(1);
        page2[0].PaymentId.Should().Be(Defaults.PaymentId);
    }

    [Fact]
    public async Task Repository_GetByUserIdAsync_Should_ReturnEmptyList_WhenUserHasNoPayments()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByUserIdAsync(InvalidData.NonExistentUserId, page: 1, pageSize: 10);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
