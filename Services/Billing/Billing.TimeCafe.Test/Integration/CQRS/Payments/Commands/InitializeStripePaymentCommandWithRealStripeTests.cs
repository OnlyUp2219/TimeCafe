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
        var userId = DefaultsGuid.UserId;
        var amount = DefaultsGuid.DefaultAmount;
        var returnUrl = StripeTestData.Configuration.DefaultReturnUrl;

        Result<InitializeStripePaymentResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                amount,
                returnUrl,
                StripeTestData.Descriptions.BalanceReplenishment));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentId.Should().NotBe(Guid.Empty);
        result.Value.ExternalPaymentId.Should().NotBeNullOrEmpty();
        result.Value.ClientSecret.Should().NotBeNullOrEmpty();
        result.Value.PublishableKey.Should().NotBeNullOrEmpty();

        result.Value.ExternalPaymentId.Should().StartWith("pi_");
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_ReturnError_WhenAmountBelowMinimum()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new InitializeStripePaymentCommand(
            DefaultsGuid.UserId,
            Defaults.BelowMinimumAmount,
            null,
            null));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_HandleMultiplePaymentsWithRealStripe()
    {
        var userId = DefaultsGuid.UserId2;

        Result<InitializeStripePaymentResponse> result1;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result1 = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                DefaultsGuid.SmallAmount,
                null,
                "First payment"));
        }

        Result<InitializeStripePaymentResponse> result2;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result2 = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                Defaults.MediumAmount,
                null,
                "Second payment"));
        }

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.ExternalPaymentId.Should().NotBe(result2.Value.ExternalPaymentId);

        result1.Value.ExternalPaymentId.Should().StartWith("pi_");
        result2.Value.ExternalPaymentId.Should().StartWith("pi_");
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_IncludePaymentMetadataInStripe()
    {
        var userId = DefaultsGuid.UserId3;
        var amount = DefaultsGuid.DefaultAmount;

        Result<InitializeStripePaymentResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                amount,
                null,
                "Metadata test"));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentId.Should().NotBe(Guid.Empty);

        var payment = await GetPaymentByIdAsync(result.Value.PaymentId);
        payment.Should().NotBeNull();
        payment!.PaymentId.Should().Be(result.Value.PaymentId);
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
