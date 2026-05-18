namespace Billing.TimeCafe.Test.Integration.CQRS.Payments.Commands;

public class ProcessStripeWebhookCommandWithRealStripeTests : BasePaymentTest
{
    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_HandleRealStripeWebhookPayload()
    {
        var userId = DefaultsGuid.UserId;
        var paymentId = DefaultsGuid.PaymentId;
        var amount = DefaultsGuid.DefaultAmount;

        await CreateBalanceAsync(userId);

        Result<InitializeStripePaymentResponse> initResult;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            initResult = await sender.Send(new InitializeStripePaymentCommand(
                userId,
                amount,
                null,
                "Webhook test"));
        }

        initResult.IsSuccess.Should().BeTrue();
        var externalPaymentId = initResult.Value.ExternalPaymentId;

        var webhook = CreateStripeSuccessWebhook(externalPaymentId!, (long)(amount * 100));

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsSuccess.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(initResult.Value.PaymentId);
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
        webhook.Type.Should().Be("checkout.session.completed");
        webhook.Data.Should().NotBeNull();
        webhook.Data.Object.Should().NotBeNull();
        webhook.Data.Object.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_HandleMissingPaymentGracefully()
    {
        var webhook = CreateStripeSuccessWebhook(StripeTestData.PaymentIntents.NonExistent, 50000);

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("не найден"));
    }
}
