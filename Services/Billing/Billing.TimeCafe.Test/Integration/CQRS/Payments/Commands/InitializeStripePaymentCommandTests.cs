namespace Billing.TimeCafe.Test.Integration.CQRS.Payments.Commands;

public class InitializeStripePaymentCommandTests : BasePaymentTest
{
    [Fact]
    public async Task Command_InitializeStripePayment_Should_CreatePayment_WhenValidRequest()
    {
        var userId = DefaultsGuid.UserId;
        var amount = DefaultsGuid.DefaultAmount;
        var returnUrl = StripeTestData.Configuration.DefaultReturnUrl;
        var description = StripeTestData.Descriptions.BalanceReplenishment;

        Result<InitializeStripePaymentResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                amount,
                returnUrl,
                description));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentId.Should().NotBe(Guid.Empty);
        result.Value.ExternalPaymentId.Should().NotBeNullOrEmpty();
        result.Value.ClientSecret.Should().NotBeNullOrEmpty();
        result.Value.PublishableKey.Should().NotBeNullOrEmpty();

        var createdPayment = await GetPaymentByIdAsync(result.Value.PaymentId);
        createdPayment.Should().NotBeNull();
        createdPayment!.UserId.Should().Be(userId);
        createdPayment.Amount.Should().Be(amount);
        createdPayment.Status.Should().Be(PaymentStatus.Pending);
        createdPayment.ExternalPaymentId.Should().Be(result.Value.ExternalPaymentId);
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_UseDefaultReturnUrl_WhenNotProvided()
    {
        var userId = DefaultsGuid.UserId2;
        var amount = DefaultsGuid.DefaultAmount;

        Result<InitializeStripePaymentResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                amount,
                null,
                "Default URL test"));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentId.Should().NotBe(Guid.Empty);
        result.Value.ExternalPaymentId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_ReturnError_WhenUserIdEmpty()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new InitializeStripePaymentCommand(
            InvalidDataGuid.EmptyUserId,
            DefaultsGuid.DefaultAmount,
            null,
            null));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_ReturnError_WhenAmountZero()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new InitializeStripePaymentCommand(
            DefaultsGuid.UserId3,
            0m,
            null,
            null));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_ReturnError_WhenAmountBelowMinimum()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new InitializeStripePaymentCommand(
            DefaultsGuid.UserId3,
            Defaults.BelowMinimumAmount,
            null,
            null));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_ReturnError_WhenAmountNegative()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new InitializeStripePaymentCommand(
            DefaultsGuid.UserId3,
            -100m,
            null,
            null));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_ReturnError_WhenReturnUrlInvalid()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new InitializeStripePaymentCommand(
            DefaultsGuid.UserId3,
            DefaultsGuid.DefaultAmount,
            "not-a-valid-url",
            null));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_HandleMultiplePaymentsPerUser()
    {
        var userId = DefaultsGuid.UserId;

        var result1 = await CreatePaymentAndInitializeAsync(userId, DefaultsGuid.SmallAmount);
        var result2 = await CreatePaymentAndInitializeAsync(userId, Defaults.MediumAmount);

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.PaymentId.Should().NotBe(result2.Value.PaymentId);

        var payments = await GetPaymentsByUserIdAsync(userId);
        payments.Should().HaveCount(2);
        payments.Should().AllSatisfy(p => p.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_StorePaymentWithCorrectDetails()
    {
        var userId = DefaultsGuid.UserId2;
        var amount = Defaults.PremiumSubscriptionAmount;
        var description = StripeTestData.Descriptions.PremiumSubscription;

        Result<InitializeStripePaymentResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                amount,
                "https://callback.com/payment",
                description));
        }

        var payment = await GetPaymentByIdAsync(result.Value.PaymentId);
        payment.Should().NotBeNull();
        payment!.PaymentMethod.Should().Be(PaymentMethod.Online);
        payment.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_CreateUniqueExternalPaymentIds()
    {
        var userId = DefaultsGuid.UserId3;

        var result1 = await CreatePaymentAndInitializeAsync(userId, 100m);
        var result2 = await CreatePaymentAndInitializeAsync(userId, 100m);

        result1.Value.ExternalPaymentId.Should().NotBe(result2.Value.ExternalPaymentId);
    }

    private async Task<Result<InitializeStripePaymentResponse>> CreatePaymentAndInitializeAsync(
        Guid userId,
        decimal amount)
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(new InitializeStripePaymentCommand(
            userId,
            amount,
            null,
            StripeTestData.Descriptions.BalanceReplenishment));
    }
}
