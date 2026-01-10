namespace Billing.TimeCafe.Test.Integration.Repository.Payment;

public class GetTotalCountByUserIdAsyncTests : BasePaymentRepositoryTest
{
    [Fact]
    public async Task Repository_GetTotalCountByUserIdAsync_Should_ReturnCountOnlyForRequestedUser()
    {
        await CreateTestPaymentAsync(paymentId: DefaultsGuid.PaymentId, userId: DefaultsGuid.UserId);
        await CreateTestPaymentAsync(paymentId: DefaultsGuid.PaymentId2, userId: DefaultsGuid.UserId);
        await CreateTestPaymentAsync(paymentId: DefaultsGuid.PaymentId3, userId: DefaultsGuid.UserId2);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var countUser1 = await repository.GetTotalCountByUserIdAsync(DefaultsGuid.UserId);
        var countUser2 = await repository.GetTotalCountByUserIdAsync(DefaultsGuid.UserId2);

        countUser1.Should().Be(2);
        countUser2.Should().Be(1);
    }
}
