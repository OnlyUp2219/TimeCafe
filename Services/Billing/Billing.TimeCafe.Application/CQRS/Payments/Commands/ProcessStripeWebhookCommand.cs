namespace Billing.TimeCafe.Application.CQRS.Payments.Commands;

public sealed record ProcessStripeWebhookCommand(
    StripeWebhookPayload Payload,
    string? SignatureHeader,
    bool BypassSignature = false) : ICommand;

public sealed class ProcessStripeWebhookCommandValidator : AbstractValidator<ProcessStripeWebhookCommand>
{
    public ProcessStripeWebhookCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Пустое тело запроса");

        RuleFor(x => x.Payload)
            .Must(p => p is { Data.Object.Id.Length: > 0 })
            .WithMessage("Платёж не найден");
    }
}

public sealed class ProcessStripeWebhookCommandHandler(
    IUnitOfWork uow,
    ISender sender,
    IPublisher publisher,
    IPublishEndpoint publishEndpoint,
    IOptionsSnapshot<StripeOptions> options,
    ILogger<ProcessStripeWebhookCommandHandler> logger) : ICommandHandler<ProcessStripeWebhookCommand>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ISender _sender = sender;
    private readonly IPublisher _publisher = publisher;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IOptionsSnapshot<StripeOptions> _options = options;
    private readonly ILogger _logger = logger;

    public async Task<Result> Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken = default)
    {
        var payload = request.Payload;

        if (string.IsNullOrWhiteSpace(payload.Type) ||
            !payload.Type.StartsWith("checkout.session.", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Ok(); // Ignored
        }

        if (!request.BypassSignature)
        {
            var webhookSecret = _options.Value.WebhookSecret;
            if (!string.IsNullOrWhiteSpace(webhookSecret) && string.IsNullOrWhiteSpace(request.SignatureHeader))
            {
                return Result.Fail(new StripeWebhookError("Missing signature header"));
            }
        }

        var paymentIntent = payload.Data?.Object;
        if (paymentIntent == null || string.IsNullOrWhiteSpace(paymentIntent.Id))
            return Result.Fail(new PaymentNotFoundError());

        var stripeObjectId = paymentIntent.Id.Trim();
        var stripePaymentIntentId = paymentIntent.PaymentIntentId?.Trim();

        string? metadataPaymentId = null;
        string? metadataUserId = null;
        string? metadataInvoiceId = null;
        paymentIntent.Metadata?.TryGetValue("paymentId", out metadataPaymentId);
        paymentIntent.Metadata?.TryGetValue("userId", out metadataUserId);
        paymentIntent.Metadata?.TryGetValue("invoiceId", out metadataInvoiceId);

        var payment = await FindPaymentAsync(stripeObjectId, stripePaymentIntentId, metadataPaymentId, metadataUserId, paymentIntent, cancellationToken);

        if (payment == null)
        {
            _logger.LogWarning("Stripe webhook payment not found: {ObjectId}", stripeObjectId);
            Guid.TryParse(metadataPaymentId, out var pid);
            return Result.Fail(new PaymentNotFoundError(pid != Guid.Empty ? pid : null));
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            if (payment.Status == PaymentStatus.Completed)
                return Result.Ok();

            _logger.LogWarning("Stripe webhook received {Type} for payment {PaymentId} in final status {Status}", 
                payload.Type, payment.PaymentId, payment.Status);

            return Result.Fail(new Error($"Нельзя изменить статус платежа из {payment.Status}"));
        }

        var result = payload.Type.ToLower() switch
        {
            "checkout.session.completed" or "checkout.session.async_payment_succeeded"
                => Guid.TryParse(metadataInvoiceId, out var invoiceId) && invoiceId != Guid.Empty
                    ? await HandleInvoiceSuccess(invoiceId, payment, paymentIntent, cancellationToken)
                    : await HandleSuccess(payment, paymentIntent, cancellationToken),

            "checkout.session.async_payment_failed"
                => await HandleFailure(payment, "Stripe: асинхронный платёж отклонён", cancellationToken),

            "checkout.session.expired"
                => await HandleCancellation(payment, "Сессия оплаты истекла", cancellationToken),

            _ => Result.Ok()
        };

        if (result.IsSuccess)
        {
            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new PaymentChangedEvent(payment.PaymentId, payment.UserId, payment.TransactionId), cancellationToken);

            if (payment.UserId.HasValue)
            {
                await _publisher.Publish(new BalanceChangedEvent(payment.UserId.Value), cancellationToken);
            }
        }

        return result;
    }

    private async Task<Payment?> FindPaymentAsync(
        string objectId,
        string? intentId,
        string? metadataPaymentId,
        string? metadataUserId,
        StripePaymentIntentObject intent,
        CancellationToken cancellationToken = default)
    {
        var payment = await _uow.Payments.GetByExternalPaymentIdAsync(objectId, cancellationToken);

        if (payment == null && !string.IsNullOrWhiteSpace(intentId))
            payment = await _uow.Payments.GetByExternalPaymentIdAsync(intentId, cancellationToken);

        if (payment == null && Guid.TryParse(metadataPaymentId, out var paymentId))
            payment = await _uow.Payments.GetByIdAsync(paymentId, cancellationToken);

        if (payment == null && Guid.TryParse(metadataUserId, out var userId))
        {
            var amount = intent.AmountTotal > 0 ? intent.AmountTotal / 100m : intent.Amount / 100m;
            var recent = await _uow.Payments.GetByUserIdAsync(userId, 1, 10, cancellationToken);
            payment = recent.FirstOrDefault(p => p.Status == PaymentStatus.Pending && Math.Abs(p.Amount - amount) <= 0.01m);
        }

        return payment;
    }

    private async Task<Result> HandleSuccess(Payment payment, StripePaymentIntentObject session, CancellationToken cancellationToken = default)
    {
        if (!payment.UserId.HasValue)
            return Result.Fail(new Error("Анонимный платеж не может пополнить баланс"));

        var amount = session.AmountTotal > 0 ? session.AmountTotal / 100m : payment.Amount;

        var adjustResult = await _sender.Send(new AdjustBalanceCommand(
            payment.UserId.Value,
            amount,
            TransactionType.Deposit,
            TransactionSource.Payment,
            payment.PaymentId,
            "Пополнение баланса через Stripe Checkout"), cancellationToken);

        if (adjustResult.IsFailed)
        {
            var error = adjustResult.Errors.First();
            if (error is not DuplicateTransactionError)
            {
                payment.MarkAsFailed(error.Message);
                await _uow.Payments.UpdateAsync(payment, cancellationToken);
                return Result.Fail(error);
            }
        }

        payment.MarkAsSucceeded(
            adjustResult.ValueOrDefault?.TransactionId ?? Guid.Empty,
            session.Created > 0 ? DateTimeOffset.FromUnixTimeSeconds(session.Created) : null);

        if (!string.IsNullOrWhiteSpace(session.PaymentIntentId))
        {
            payment.UpdateExternalData(System.Text.Json.JsonSerializer.Serialize(new { paymentIntentId = session.PaymentIntentId }));
        }

        await _uow.Payments.UpdateAsync(payment, cancellationToken);
        return Result.Ok();
    }

    private async Task<Result> HandleInvoiceSuccess(Guid invoiceId, Payment payment, StripePaymentIntentObject session, CancellationToken cancellationToken = default)
    {
        var invoice = await _uow.Invoices.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
            return Result.Fail(new InvoiceNotFoundError());

        var payResult = invoice.Pay(PaymentMethod.Online, session.Created > 0 ? DateTimeOffset.FromUnixTimeSeconds(session.Created) : null);
        if (payResult.IsFailed)
            return payResult;

        payment.MarkAsSucceeded(
            Guid.Empty,
            session.Created > 0 ? DateTimeOffset.FromUnixTimeSeconds(session.Created) : null);

        if (!string.IsNullOrWhiteSpace(session.PaymentIntentId))
        {
            payment.UpdateExternalData(System.Text.Json.JsonSerializer.Serialize(new { paymentIntentId = session.PaymentIntentId }));
        }

        await _uow.Invoices.UpdateAsync(invoice, cancellationToken);
        await _uow.Payments.UpdateAsync(payment, cancellationToken);

        if (invoice.UserId.HasValue)
        {
            var balance = await _uow.Balances.GetByIdAsync(invoice.UserId.Value, cancellationToken);
            if (balance != null)
            {
                balance.RecordSpent(invoice.TotalAmount);
                await _uow.Balances.UpdateAsync(balance, cancellationToken);
            }
        }

        await _publisher.Publish(new InvoiceChangedEvent(invoice.InvoiceId, invoice.UserId, invoice.VisitId), cancellationToken);

        await _publishEndpoint.Publish(new InvoicePaidEvent
        {
            InvoiceId = invoice.InvoiceId,
            VisitId = invoice.VisitId,
            UserId = invoice.UserId,
            Amount = invoice.TotalAmount,
            PaidAt = invoice.PaidAt ?? DateTimeOffset.UtcNow
        }, cancellationToken);

        return Result.Ok();
    }

    private async Task<Result> HandleFailure(Payment payment, string message, CancellationToken cancellationToken = default)
    {
        payment.MarkAsFailed(message);
        await _uow.Payments.UpdateAsync(payment, cancellationToken);
        return Result.Ok();
    }

    private async Task<Result> HandleCancellation(Payment payment, string message, CancellationToken cancellationToken = default)
    {
        payment.MarkAsCancelled(message);
        await _uow.Payments.UpdateAsync(payment, cancellationToken);
        return Result.Ok();
    }
}
