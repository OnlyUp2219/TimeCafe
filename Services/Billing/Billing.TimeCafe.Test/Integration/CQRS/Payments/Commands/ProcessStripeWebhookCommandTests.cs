namespace Billing.TimeCafe.Test.Integration.CQRS.Payments.Commands;

public class ProcessStripeWebhookCommandTests : BasePaymentTest
{
    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_CompletePayment_WhenSuccessEvent()
    {
        var userId = Defaults.UserId;
        var paymentId = Defaults.PaymentId;
        var externalPaymentId = StripeTestData.PaymentIntents.Default;
        var amount = Defaults.DefaultAmount;

        await CreateBalanceAsync(userId);
        await CreatePaymentAsync(paymentId, userId, amount, PaymentStatus.Pending, externalPaymentId);

        var webhook = CreateStripeSuccessWebhook(externalPaymentId, (long)(amount * 100));

        ProcessStripeWebhookResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.Success.Should().BeTrue();
        result.StatusCode.Should().BeNull();

        var payment = await GetPaymentByIdAsync(paymentId);
        payment!.Status.Should().Be(PaymentStatus.Completed);
        payment.CompletedAt.Should().NotBeNull();

        var balance = await GetBalanceByUserIdAsync(userId);
        balance!.CurrentBalance.Should().Be(amount);
        balance.TotalDeposited.Should().Be(amount);
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_FailPayment_WhenFailedEvent()
    {
        var userId = Defaults.UserId2;
        var paymentId = Defaults.PaymentId2;
        var externalPaymentId = StripeTestData.PaymentIntents.Default;

        await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Pending, externalPaymentId);

        var webhook = CreateStripeFailedWebhook(externalPaymentId);

        ProcessStripeWebhookResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.Success.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(paymentId);
        payment!.Status.Should().Be(PaymentStatus.Failed);
        payment.ErrorMessage.Should().Contain("Stripe");
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_CancelPayment_WhenCancelledEvent()
    {
        var userId = Defaults.UserId3;
        var paymentId = Defaults.PaymentId3;
        var externalPaymentId = StripeTestData.PaymentIntents.Secondary;

        await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Pending, externalPaymentId);

        var webhook = CreateStripeCancelledWebhook(externalPaymentId);

        ProcessStripeWebhookResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.Success.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(paymentId);
        payment!.Status.Should().Be(PaymentStatus.Cancelled);
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_ReturnAlreadyProcessed_WhenPaymentAlreadyCompleted()
    {
        var userId = Defaults.UserId;
        var paymentId = Defaults.PaymentId;
        var externalPaymentId = StripeTestData.PaymentIntents.Default;

        await CreatePaymentAsync(
            paymentId,
            userId,
            Defaults.DefaultAmount,
            PaymentStatus.Completed,
            externalPaymentId);

        var webhook = CreateStripeSuccessWebhook(externalPaymentId, (long)(Defaults.DefaultAmount * 100));

        ProcessStripeWebhookResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("уже");
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_ReturnError_WhenPaymentNotFound()
    {
        var webhook = CreateStripeSuccessWebhook(StripeTestData.PaymentIntents.NonExistent, (long)(Defaults.DefaultAmount * 100));

        ProcessStripeWebhookResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.Success.Should().BeFalse();
        result.Code.Should().Be("PaymentNotFound");
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_IgnoreUnknownEventType()
    {
        var userId = Defaults.UserId;
        var paymentId = Defaults.PaymentId;
        var externalPaymentId = StripeTestData.PaymentIntents.Default;

        await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Pending, externalPaymentId);

        var webhook = new StripeWebhookPayload
        {
            Type = StripeTestData.Events.Refunded,
            Data = new StripeWebhookData
            {
                Object = new StripePaymentIntentObject
                {
                    Id = externalPaymentId,
                    Amount = (long)(Defaults.DefaultAmount * 100),
                    Status = StripeTestData.Statuses.Refunded,
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            }
        };

        ProcessStripeWebhookResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("проигнорировано");
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_ReturnError_WhenPayloadNull()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new ProcessStripeWebhookCommand(null!, null));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_HandleAmountMismatchWarning_ButStillProcess()
    {
        var userId = Defaults.UserId2;
        var paymentId = Defaults.PaymentId2;
        var externalPaymentId = StripeTestData.PaymentIntents.Default;
        var paymentAmount = Defaults.DefaultAmount;
        var stripeAmount = 600m;

        await CreateBalanceAsync(userId);
        await CreatePaymentAsync(paymentId, userId, paymentAmount, PaymentStatus.Pending, externalPaymentId);

        var webhook = CreateStripeSuccessWebhook(externalPaymentId, (long)(stripeAmount * 100));

        ProcessStripeWebhookResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.Success.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(paymentId);
        payment!.Status.Should().Be(PaymentStatus.Completed);

        var balance = await GetBalanceByUserIdAsync(userId);
        balance!.CurrentBalance.Should().Be(stripeAmount);
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_SkipProcessingWhenNoWebhookSecret()
    {
        var webhook = CreateStripeSuccessWebhook(StripeTestData.PaymentIntents.Default, (long)(Defaults.DefaultAmount * 100));

        ProcessStripeWebhookResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.Success.Should().BeFalse();
        result.Code.Should().Be("PaymentNotFound");
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_LinkPaymentByMetadata_WhenExternalIdNotMatched()
    {
        var userId = Defaults.UserId3;
        var paymentId = Defaults.PaymentId4;
        var externalPaymentId = StripeTestData.PaymentIntents.NewExternal;

        await CreateBalanceAsync(userId);
        await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Pending, null);

        var webhook = new StripeWebhookPayload
        {
            Type = "payment_intent.succeeded",
            Data = new StripeWebhookData
            {
                Object = new StripePaymentIntentObject
                {
                    Id = externalPaymentId,
                    Amount = (long)(Defaults.DefaultAmount * 100),
                    Status = "succeeded",
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Metadata = new Dictionary<string, string> { { "paymentId", paymentId.ToString() } }
                }
            }
        };

        ProcessStripeWebhookResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.Success.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(paymentId);
        payment!.Status.Should().Be(PaymentStatus.Completed);
        payment.ExternalPaymentId.Should().Be(externalPaymentId);
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_UpdateExternalPaymentId_OnSuccess()
    {
        var userId = Defaults.UserId;
        var paymentId = Defaults.PaymentId;
        var newExternalPaymentId = StripeTestData.PaymentIntents.External;

        await CreateBalanceAsync(userId);
        await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Pending, newExternalPaymentId);

        var webhook = CreateStripeSuccessWebhook(newExternalPaymentId, (long)(Defaults.DefaultAmount * 100));

        ProcessStripeWebhookResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.Success.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(paymentId);
        payment!.ExternalPaymentId.Should().Be(newExternalPaymentId);
    }
}
