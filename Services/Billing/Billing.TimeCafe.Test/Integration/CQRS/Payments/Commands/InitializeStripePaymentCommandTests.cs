namespace Billing.TimeCafe.Test.Integration.CQRS.Payments.Commands;

public class InitializeStripePaymentCommandTests : BasePaymentTest
{
    [Fact]
    public async Task Command_InitializeStripePayment_Should_CreatePayment_WhenValidRequest()
    {
        var userId = Defaults.UserId;
        var amount = Defaults.DefaultAmount;
        var returnUrl = StripeTestData.Configuration.DefaultReturnUrl;
        var description = StripeTestData.Descriptions.BalanceReplenishment;

        InitializeStripePaymentResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                amount,
                returnUrl,
                description));
        }

        result.Success.Should().BeTrue();
        result.Code.Should().BeNull();
        result.StatusCode.Should().BeNull();
        result.PaymentId.Should().NotBe(Guid.Empty);
        result.ExternalPaymentId.Should().NotBeNullOrEmpty();
        result.ClientSecret.Should().NotBeNullOrEmpty();
        result.PublishableKey.Should().NotBeNullOrEmpty();

        var createdPayment = await GetPaymentByIdAsync(result.PaymentId!.Value);
        createdPayment.Should().NotBeNull();
        createdPayment!.UserId.Should().Be(userId);
        createdPayment.Amount.Should().Be(amount);
        createdPayment.Status.Should().Be(PaymentStatus.Pending);
        createdPayment.ExternalPaymentId.Should().Be(result.ExternalPaymentId);
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_UseDefaultReturnUrl_WhenNotProvided()
    {
        var userId = Defaults.UserId2;
        var amount = Defaults.DefaultAmount;

        InitializeStripePaymentResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                amount,
                null,
                "Default URL test"));
        }

        result.Success.Should().BeTrue();
        result.PaymentId.Should().NotBe(Guid.Empty);
        result.ExternalPaymentId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_ReturnError_WhenUserIdEmpty()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new InitializeStripePaymentCommand(
            InvalidData.EmptyUserId,
            Defaults.DefaultAmount,
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
            Defaults.UserId3,
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
            Defaults.UserId3,
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
            Defaults.UserId3,
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
            Defaults.UserId3,
            Defaults.DefaultAmount,
            "not-a-valid-url",
            null));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_HandleMultiplePaymentsPerUser()
    {
        var userId = Defaults.UserId;

        var result1 = await CreatePaymentAndInitializeAsync(userId, Defaults.SmallAmount);
        var result2 = await CreatePaymentAndInitializeAsync(userId, Defaults.MediumAmount);

        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue();
        result1.PaymentId.Should().NotBe(result2.PaymentId!.Value);

        var payments = await GetPaymentsByUserIdAsync(userId);
        payments.Should().HaveCount(2);
        payments.Should().AllSatisfy(p => p.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_StorePaymentWithCorrectDetails()
    {
        var userId = Defaults.UserId2;
        var amount = Defaults.PremiumSubscriptionAmount;
        var description = StripeTestData.Descriptions.PremiumSubscription;

        InitializeStripePaymentResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                amount,
                "https://callback.com/payment",
                description));
        }

        var payment = await GetPaymentByIdAsync(result.PaymentId!.Value);
        payment.Should().NotBeNull();
        payment!.PaymentMethod.Should().Be(PaymentMethod.Online);
        payment.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Command_InitializeStripePayment_Should_CreateUniqueExternalPaymentIds()
    {
        var userId = Defaults.UserId3;

        var result1 = await CreatePaymentAndInitializeAsync(userId, 100m);
        var result2 = await CreatePaymentAndInitializeAsync(userId, 100m);

        result1.ExternalPaymentId.Should().NotBe(result2.ExternalPaymentId);
    }

    private async Task<InitializeStripePaymentResult> CreatePaymentAndInitializeAsync(
        Guid userId,
        decimal amount)
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(new InitializeStripePaymentCommand(
            userId.ToString(),
            amount,
            null,
            StripeTestData.Descriptions.BalanceReplenishment));
    }

    private async Task<InitializeStripePaymentResult> CreatePaymentAndInitializeAsync(
        string userId,
        decimal amount)
        => await CreatePaymentAndInitializeAsync(Guid.Parse(userId), amount);
}
