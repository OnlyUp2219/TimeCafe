namespace Billing.TimeCafe.Test.Integration.CQRS.Payments.Commands;

public class ProcessStripeWebhookCommandWithRealStripeTests : BasePaymentTest
{
    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_HandleRealStripeWebhookPayload()
    {
        var userId = Defaults.UserId;
        var paymentId = Defaults.PaymentId;
        var amount = Defaults.DefaultAmount;

        await CreateBalanceAsync(userId);

        InitializeStripePaymentResult initResult;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            initResult = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                amount,
                null,
                "Webhook test"));
        }

        initResult.Success.Should().BeTrue();
        var externalPaymentId = initResult.ExternalPaymentId;

        var webhook = CreateStripeSuccessWebhook(externalPaymentId!, (long)(amount * 100));

        ProcessStripeWebhookResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.Success.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(initResult.PaymentId!.Value);
        payment.Should().NotBeNull();
        payment!.ExternalPaymentId.Should().Be(externalPaymentId);

        var balance = await GetBalanceByUserIdAsync(userId);
        balance!.CurrentBalance.Should().Be(amount);
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_ValidateWebhookFormat()
    {
        var webhook = CreateStripeSuccessWebhook(StripeTestData.PaymentIntents.Default, 50000);

        webhook.Should().NotBeNull();
        webhook.Type.Should().Be(StripeTestData.Events.Succeeded);
        webhook.Data.Should().NotBeNull();
        webhook.Data.Object.Should().NotBeNull();
        webhook.Data.Object.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_HandleMissingPaymentGracefully()
    {
        var webhook = CreateStripeSuccessWebhook(StripeTestData.PaymentIntents.NonExistent, 50000);

        ProcessStripeWebhookResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.Success.Should().BeFalse();
        result.Code.Should().Be("PaymentNotFound");
    }
}
