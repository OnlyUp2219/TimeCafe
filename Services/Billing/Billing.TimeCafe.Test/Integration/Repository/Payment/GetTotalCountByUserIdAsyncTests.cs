namespace Billing.TimeCafe.Test.Integration.Repository.Payment;

public class GetTotalCountByUserIdAsyncTests : BasePaymentRepositoryTest
{
    [Fact]
    public async Task Repository_GetTotalCountByUserIdAsync_Should_ReturnCountOnlyForRequestedUser()
    {
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId, userId: Defaults.UserId);
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId2, userId: Defaults.UserId);
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId3, userId: Defaults.UserId2);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var countUser1 = await repository.GetTotalCountByUserIdAsync(Defaults.UserId);
        var countUser2 = await repository.GetTotalCountByUserIdAsync(Defaults.UserId2);

        countUser1.Should().Be(2);
        countUser2.Should().Be(1);
    }
}
