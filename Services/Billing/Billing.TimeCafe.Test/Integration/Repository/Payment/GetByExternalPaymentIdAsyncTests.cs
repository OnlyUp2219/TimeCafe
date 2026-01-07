namespace Billing.TimeCafe.Test.Integration.Repository.Payment;

public class GetByExternalPaymentIdAsyncTests : BasePaymentRepositoryTest
{
    [Fact]
    public async Task Repository_GetByExternalPaymentIdAsync_Should_ReturnPayment_WhenExists()
    {
        await CreateTestPaymentAsync(
            paymentId: Defaults.PaymentId,
            externalPaymentId: Defaults.StripePaymentIntentId);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByExternalPaymentIdAsync(Defaults.StripePaymentIntentId);

        result.Should().NotBeNull();
        result!.PaymentId.Should().Be(Defaults.PaymentId);
        result.ExternalPaymentId.Should().Be(Defaults.StripePaymentIntentId);
    }

    [Fact]
    public async Task Repository_GetByExternalPaymentIdAsync_Should_ReturnNull_WhenNotExists()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByExternalPaymentIdAsync(Defaults.StripeNonExistentPaymentIntentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByExternalPaymentIdAsync_Should_ReturnNull_WhenExternalPaymentIdIsEmpty()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByExternalPaymentIdAsync(string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByExternalPaymentIdAsync_Should_ReturnNull_WhenExternalPaymentIdIsNull()
    {
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByExternalPaymentIdAsync(null!);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByExternalPaymentIdAsync_Should_ReturnNull_WhenPaymentHasNoExternalPaymentId()
    {
        await CreateTestPaymentAsync(paymentId: Defaults.PaymentId, externalPaymentId: null);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByExternalPaymentIdAsync(Defaults.StripePaymentIntentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetByExternalPaymentIdAsync_Should_ReturnPaymentWithCorrectStatus()
    {
        await CreateTestPaymentAsync(
            paymentId: Defaults.PaymentId,
            externalPaymentId: Defaults.StripePaymentIntentId,
            status: PaymentStatus.Completed);

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var result = await repository.GetByExternalPaymentIdAsync(Defaults.StripePaymentIntentId);

        result.Should().NotBeNull();
        result!.Status.Should().Be(PaymentStatus.Completed);
    }
}
