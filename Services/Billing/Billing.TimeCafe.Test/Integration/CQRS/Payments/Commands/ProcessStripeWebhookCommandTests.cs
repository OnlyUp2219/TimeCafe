namespace Billing.TimeCafe.Test.Integration.CQRS.Payments.Commands;

public class ProcessStripeWebhookCommandTests : BasePaymentTest
{
    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_CompletePayment_WhenCheckoutSessionCompletedEvent()
    {
        var userId = DefaultsGuid.UserId3;
        var paymentId = DefaultsGuid.PaymentId5;
        const string checkoutSessionId = "cs_test_checkout_session";
        const decimal amount = 250m;

        await CreateBalanceAsync(userId);
        await CreatePaymentAsync(paymentId, userId, amount, PaymentStatus.Pending, checkoutSessionId);

        var webhook = CreateStripeCheckoutCompletedWebhook(checkoutSessionId, amount * 100, "pi_test_123");

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsSuccess.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(paymentId);
        payment!.Status.Should().Be(PaymentStatus.Completed);
        payment.TransactionId.Should().NotBeNull();

        var balance = await GetBalanceByUserIdAsync(userId);
        balance!.CurrentBalance.Should().Be(amount);
        balance.TotalDeposited.Should().Be(amount);
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_CompletePayment_WhenSuccessEvent()
    {
        var userId = DefaultsGuid.UserId;
        var paymentId = DefaultsGuid.PaymentId;
        var externalPaymentId = StripeTestData.PaymentIntents.Default;
        var amount = DefaultsGuid.DefaultAmount;

        await CreateBalanceAsync(userId);
        await CreatePaymentAsync(paymentId, userId, amount, PaymentStatus.Pending, externalPaymentId);

        var webhook = CreateStripeSuccessWebhook(externalPaymentId, (long)(amount * 100));

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsSuccess.Should().BeTrue();

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
        var userId = DefaultsGuid.UserId2;
        var paymentId = DefaultsGuid.PaymentId2;
        var externalPaymentId = StripeTestData.PaymentIntents.Default;

        await CreatePaymentAsync(paymentId, userId, DefaultsGuid.DefaultAmount, PaymentStatus.Pending, externalPaymentId);

        var webhook = CreateStripeFailedWebhook(externalPaymentId);

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsSuccess.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(paymentId);
        payment!.Status.Should().Be(PaymentStatus.Failed);
        payment.ErrorMessage.Should().Contain("Stripe");
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_CancelPayment_WhenCancelledEvent()
    {
        var userId = DefaultsGuid.UserId3;
        var paymentId = DefaultsGuid.PaymentId3;
        var externalPaymentId = StripeTestData.PaymentIntents.Secondary;

        await CreatePaymentAsync(paymentId, userId, DefaultsGuid.DefaultAmount, PaymentStatus.Pending, externalPaymentId);

        var webhook = CreateStripeCancelledWebhook(externalPaymentId);

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsSuccess.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(paymentId);
        payment!.Status.Should().Be(PaymentStatus.Cancelled);
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_ReturnAlreadyProcessed_WhenPaymentAlreadyCompleted()
    {
        var userId = DefaultsGuid.UserId;
        var paymentId = DefaultsGuid.PaymentId;
        var externalPaymentId = StripeTestData.PaymentIntents.Default;

        await CreatePaymentAsync(
            paymentId,
            userId,
            DefaultsGuid.DefaultAmount,
            PaymentStatus.Completed,
            externalPaymentId);

        var webhook = CreateStripeSuccessWebhook(externalPaymentId, (long)(DefaultsGuid.DefaultAmount * 100));

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_ReturnError_WhenPaymentNotFound()
    {
        var webhook = CreateStripeSuccessWebhook(StripeTestData.PaymentIntents.NonExistent, (long)(DefaultsGuid.DefaultAmount * 100));

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("не найден"));
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_IgnoreUnknownEventType()
    {
        var userId = DefaultsGuid.UserId;
        var paymentId = DefaultsGuid.PaymentId;
        var externalPaymentId = StripeTestData.PaymentIntents.Default;

        await CreatePaymentAsync(paymentId, userId, DefaultsGuid.DefaultAmount, PaymentStatus.Pending, externalPaymentId);

        var webhook = new StripeWebhookPayload
        {
            Type = StripeTestData.Events.Refunded,
            Data = new StripeWebhookData
            {
                Object = new StripePaymentIntentObject
                {
                    Id = externalPaymentId,
                    Amount = (long)(DefaultsGuid.DefaultAmount * 100),
                    Status = StripeTestData.Statuses.Refunded,
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            }
        };

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsSuccess.Should().BeTrue();
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
        var userId = DefaultsGuid.UserId2;
        var paymentId = DefaultsGuid.PaymentId2;
        var externalPaymentId = StripeTestData.PaymentIntents.Default;
        var paymentAmount = DefaultsGuid.DefaultAmount;
        const decimal stripeAmount = 600m;

        await CreateBalanceAsync(userId);
        await CreatePaymentAsync(paymentId, userId, paymentAmount, PaymentStatus.Pending, externalPaymentId);

        var webhook = CreateStripeSuccessWebhook(externalPaymentId, (long)(stripeAmount * 100));

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsSuccess.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(paymentId);
        payment!.Status.Should().Be(PaymentStatus.Completed);

        var balance = await GetBalanceByUserIdAsync(userId);
        balance!.CurrentBalance.Should().Be(stripeAmount);
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_SkipProcessingWhenNoWebhookSecret()
    {
        var webhook = CreateStripeSuccessWebhook(StripeTestData.PaymentIntents.Default, (long)(DefaultsGuid.DefaultAmount * 100));

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("не найден"));
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_LinkPaymentByMetadata_WhenExternalIdNotMatched()
    {
        var userId = DefaultsGuid.UserId3;
        var paymentId = DefaultsGuid.PaymentId4;
        var externalPaymentId = StripeTestData.PaymentIntents.NewExternal;

        await CreateBalanceAsync(userId);
        await CreatePaymentAsync(paymentId, userId, DefaultsGuid.DefaultAmount, PaymentStatus.Pending, null);

        var webhook = new StripeWebhookPayload
        {
            Type = "checkout.session.completed",
            Data = new StripeWebhookData
            {
                Object = new StripePaymentIntentObject
                {
                    Id = $"cs_test_{externalPaymentId}",
                    AmountTotal = (long)(DefaultsGuid.DefaultAmount * 100),
                    PaymentIntentId = externalPaymentId,
                    Status = "complete",
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Metadata = new Dictionary<string, string> { { "paymentId", paymentId.ToString() } }
                }
            }
        };

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsSuccess.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(paymentId);
        payment!.Status.Should().Be(PaymentStatus.Completed);
        payment.ExternalData.Should().Contain(externalPaymentId);
    }

    [Fact]
    public async Task Command_ProcessStripeWebhook_Should_UpdateExternalPaymentId_OnSuccess()
    {
        var userId = DefaultsGuid.UserId;
        var paymentId = DefaultsGuid.PaymentId;
        var newExternalPaymentId = StripeTestData.PaymentIntents.External;

        await CreateBalanceAsync(userId);
        await CreatePaymentAsync(paymentId, userId, DefaultsGuid.DefaultAmount, PaymentStatus.Pending, newExternalPaymentId);

        var webhook = CreateStripeSuccessWebhook(newExternalPaymentId, (long)(DefaultsGuid.DefaultAmount * 100));

        Result result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new ProcessStripeWebhookCommand(webhook, null));
        }

        result.IsSuccess.Should().BeTrue();

        var payment = await GetPaymentByIdAsync(paymentId);
        payment!.ExternalPaymentId.Should().Be(newExternalPaymentId);
    }
}
