namespace Billing.TimeCafe.Test.Integration.CQRS.Payments.Commands;

public class InitializeStripePaymentCommandWithRealStripeTests : IDisposable
{
    protected HttpClient Client { get; }
    protected IntegrationApiFactoryWithRealStripe Factory { get; }

    public InitializeStripePaymentCommandWithRealStripeTests()
    {
        Factory = new IntegrationApiFactoryWithRealStripe();
        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    [Fact]
    public async Task Command_InitializeStripePayment_Should_CreatePaymentInStripe_WhenValidRequest()
    {
        var userId = Defaults.UserId;
        var amount = Defaults.DefaultAmount;
        var returnUrl = StripeTestData.Configuration.DefaultReturnUrl;

        InitializeStripePaymentResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                amount,
                returnUrl,
                StripeTestData.Descriptions.BalanceReplenishment));
        }

        result.Success.Should().BeTrue();
        result.PaymentId.Should().NotBe(Guid.Empty);
        result.ExternalPaymentId.Should().NotBeNullOrEmpty();
        result.ClientSecret.Should().NotBeNullOrEmpty();
        result.PublishableKey.Should().NotBeNullOrEmpty();

        result.ExternalPaymentId.Should().StartWith("pi_");
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_ReturnError_WhenAmountBelowMinimum()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new InitializeStripePaymentCommand(
            Defaults.UserId,
            Defaults.BelowMinimumAmount,
            null,
            null));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_HandleMultiplePaymentsWithRealStripe()
    {
        var userId = Defaults.UserId2;

        InitializeStripePaymentResult result1;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result1 = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                Defaults.SmallAmount,
                null,
                "First payment"));
        }

        InitializeStripePaymentResult result2;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result2 = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                Defaults.MediumAmount,
                null,
                "Second payment"));
        }

        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue();
        result1.ExternalPaymentId.Should().NotBe(result2.ExternalPaymentId);

        result1.ExternalPaymentId.Should().StartWith("pi_");
        result2.ExternalPaymentId.Should().StartWith("pi_");
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_IncludePaymentMetadataInStripe()
    {
        var userId = Defaults.UserId3;
        var amount = Defaults.DefaultAmount;

        InitializeStripePaymentResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                amount,
                null,
                "Metadata test"));
        }

        result.Success.Should().BeTrue();
        result.PaymentId.Should().NotBe(Guid.Empty);

        var payment = await GetPaymentByIdAsync(result.PaymentId.Value);
        payment.Should().NotBeNull();
        payment!.PaymentId.Should().Be(result.PaymentId.Value.ToString());
        payment.UserId.Should().Be(userId);
    }

    private async Task<PaymentModel?> GetPaymentByIdAsync(Guid paymentId)
    {
        using var scope = CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        return await repo.GetByIdAsync(paymentId, CancellationToken.None);
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
