namespace Billing.TimeCafe.Application.CQRS.Payments.Commands;

public record ProcessStripeWebhookCommand(
    StripeWebhookPayload Payload,
    string? SignatureHeader) : IRequest<ProcessStripeWebhookResult>;

public record ProcessStripeWebhookResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static ProcessStripeWebhookResult Unauthorized() =>
        new(false, Code: "Unauthorized", Message: "Некорректная авторизация вебхука", StatusCode: 401);

    public static ProcessStripeWebhookResult PaymentNotFound() =>
        new(false, Code: "PaymentNotFound", Message: "Платёж не найден", StatusCode: 404);

    public static ProcessStripeWebhookResult AlreadyProcessed() =>
        new(true, Message: "Платёж уже обработан");

    public static ProcessStripeWebhookResult Completed(string? message = null) =>
        new(true, Message: message ?? "Платёж обработан");

    public static ProcessStripeWebhookResult ProviderError(string message) =>
        new(false, Code: "PaymentProcessingError", Message: message, StatusCode: 502);

    public static ProcessStripeWebhookResult Ignored() =>
        new(true, Message: "Событие проигнорировано");
}

public class ProcessStripeWebhookCommandValidator : AbstractValidator<ProcessStripeWebhookCommand>
{
    public ProcessStripeWebhookCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Пустое тело запроса");

        RuleFor(x => x.Payload)
            .Must(p => p is { Data: { Object: { Id.Length: > 0 } } })
            .WithMessage("Платёж не найден");
    }
}

public class ProcessStripeWebhookCommandHandler(
    IPaymentRepository paymentRepository,
    ISender sender,
    IOptions<StripeOptions> options,
    ILogger<ProcessStripeWebhookCommandHandler> logger) : IRequestHandler<ProcessStripeWebhookCommand, ProcessStripeWebhookResult>
{
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly ISender _sender = sender;
    private readonly IOptions<StripeOptions> _options = options;
    private readonly ILogger _logger = logger;

    public async Task<ProcessStripeWebhookResult> Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        if (request.Payload is null)
        {
            _logger.LogWarning("Stripe webhook: empty payload");
            return ProcessStripeWebhookResult.PaymentNotFound();
        }

        var payload = request.Payload;

        var webhookSecret = _options.Value.WebhookSecret;
        if (!string.IsNullOrWhiteSpace(webhookSecret) && string.IsNullOrWhiteSpace(request.SignatureHeader))
        {
            _logger.LogWarning("Stripe webhook: missing signature header");
            return ProcessStripeWebhookResult.Unauthorized();
        }

        var paymentIntent = payload.Data?.Object;
        if (paymentIntent == null || string.IsNullOrWhiteSpace(paymentIntent.Id))
            return ProcessStripeWebhookResult.PaymentNotFound();

        var stripeObjectId = paymentIntent.Id.Trim();
        var stripePaymentIntentId = paymentIntent.PaymentIntentId?.Trim();
        string? metadataPaymentId = null;
        string? metadataUserId = null;
        paymentIntent.Metadata?.TryGetValue("paymentId", out metadataPaymentId);
        paymentIntent.Metadata?.TryGetValue("userId", out metadataUserId);

        _logger.LogInformation(
            "Stripe webhook received: Type={Type}, ObjectId={ObjectId}, PaymentIntentId={PaymentIntentId}, MetadataPaymentId={MetadataPaymentId}, MetadataUserId={MetadataUserId}",
            payload.Type,
            stripeObjectId,
            stripePaymentIntentId,
            metadataPaymentId,
            metadataUserId);

        var payment = await _paymentRepository.GetByExternalPaymentIdAsync(stripeObjectId, cancellationToken)
            .ConfigureAwait(false);

        if (payment == null && !string.IsNullOrWhiteSpace(stripePaymentIntentId))
        {
            payment = await _paymentRepository.GetByExternalPaymentIdAsync(stripePaymentIntentId, cancellationToken)
                .ConfigureAwait(false);
        }

        if (payment == null && Guid.TryParse(metadataPaymentId, out var paymentId))
        {
            payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken).ConfigureAwait(false);
        }

        if (payment == null && Guid.TryParse(metadataUserId, out var metadataUserGuid))
        {
            var amountFromStripe = paymentIntent.AmountTotal > 0
                ? paymentIntent.AmountTotal / 100m
                : paymentIntent.Amount / 100m;

            var recentUserPayments = await _paymentRepository.GetByUserIdAsync(metadataUserGuid, 1, 20, cancellationToken)
                .ConfigureAwait(false);

            payment = recentUserPayments
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault(p =>
                    p.Status == PaymentStatus.Pending &&
                    (string.Equals(p.ExternalPaymentId, stripeObjectId, StringComparison.OrdinalIgnoreCase)
                     || (!string.IsNullOrWhiteSpace(stripePaymentIntentId)
                         && string.Equals(p.ExternalPaymentId, stripePaymentIntentId, StringComparison.OrdinalIgnoreCase))
                     || Math.Abs(p.Amount - amountFromStripe) <= 0.01m));
        }

        if (payment == null)
        {
            _logger.LogWarning(
                "Stripe webhook payment not found: Type={Type}, ObjectId={ObjectId}, PaymentIntentId={PaymentIntentId}, MetadataPaymentId={MetadataPaymentId}, MetadataUserId={MetadataUserId}",
                payload.Type,
                stripeObjectId,
                stripePaymentIntentId,
                metadataPaymentId,
                metadataUserId);
            return ProcessStripeWebhookResult.PaymentNotFound();
        }

        if (payment.Status == PaymentStatus.Completed)
            return ProcessStripeWebhookResult.AlreadyProcessed();

        if (string.Equals(payload.Type, "checkout.session.completed", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleCheckoutSessionCompleted(payment, paymentIntent, cancellationToken);
        }

        if (string.Equals(payload.Type, "payment_intent.succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleSuccessfulPayment(payment, paymentIntent, cancellationToken);
        }

        if (string.Equals(payload.Type, "payment_intent.payment_failed", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleFailedPayment(payment, paymentIntent, cancellationToken);
        }

        if (string.Equals(payload.Type, "payment_intent.canceled", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleCancelledPayment(payment, paymentIntent, cancellationToken);
        }

        _logger.LogInformation("Stripe: event {Event} for payment {PaymentId} skipped", payload.Type, payment.PaymentId);
        return ProcessStripeWebhookResult.Ignored();
    }

    private async Task<ProcessStripeWebhookResult> HandleCheckoutSessionCompleted(
        Payment payment,
        StripePaymentIntentObject session,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stripe webhook: checkout.session.completed for payment {PaymentId}, amount_total={AmountTotal}",
            payment.PaymentId, session.AmountTotal);

        if (payment.Status == PaymentStatus.Completed)
            return ProcessStripeWebhookResult.AlreadyProcessed();

        payment.CompletedAt = ResolveCompletedAt(session.Created);

        if (!string.IsNullOrWhiteSpace(session.PaymentIntentId))
        {
            payment.ExternalData = System.Text.Json.JsonSerializer.Serialize(new
            {
                paymentIntentId = session.PaymentIntentId
            });
        }

        await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Stripe: checkout session {SessionId} completed for user {UserId}",
            session.Id,
            payment.UserId);

        return ProcessStripeWebhookResult.Completed("Checkout session синхронизирована");
    }

    private static DateTimeOffset ResolveCompletedAt(long createdUnix)
    {
        try
        {
            if (createdUnix > 0)
                return DateTimeOffset.FromUnixTimeSeconds(createdUnix);
        }
        catch
        {
        }

        return DateTimeOffset.UtcNow;
    }

    private async Task<ProcessStripeWebhookResult> HandleSuccessfulPayment(
        Payment payment,
        StripePaymentIntentObject paymentIntent,
        CancellationToken cancellationToken)
    {
        var amountFromStripe = paymentIntent.Amount / 100m;

        if (Math.Abs(amountFromStripe - payment.Amount) > 0.01m)
        {
            _logger.LogWarning("Stripe: amount mismatch {WebhookAmount} vs {Expected}", amountFromStripe, payment.Amount);
        }

        var adjustResult = await _sender.Send(new AdjustBalanceCommand(
            payment.UserId.ToString(),
            amountFromStripe,
            TransactionType.Deposit,
            TransactionSource.Payment,
            payment.PaymentId.ToString(),
            "Пополнение баланса через Stripe"), cancellationToken).ConfigureAwait(false);

        if (!adjustResult.Success)
        {
            if (string.Equals(adjustResult.Code, "DuplicateTransaction", StringComparison.OrdinalIgnoreCase))
            {
                payment.Status = PaymentStatus.Completed;
                payment.CompletedAt = ResolveCompletedAt(paymentIntent.Created);
                payment.ExternalPaymentId = paymentIntent.Id;
                payment.ErrorMessage = null;
                await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);
                return ProcessStripeWebhookResult.Completed("Платёж уже был зачислен ранее");
            }

            payment.Status = PaymentStatus.Failed;
            payment.ErrorMessage = adjustResult.Message ?? "Не удалось зачислить оплату";
            await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);
            return ProcessStripeWebhookResult.ProviderError(adjustResult.Message ?? "Ошибка зачисления платежа");
        }

        payment.Status = PaymentStatus.Completed;
        payment.CompletedAt = ResolveCompletedAt(paymentIntent.Created);
        payment.TransactionId = adjustResult.Transaction?.TransactionId;
        payment.ExternalPaymentId = paymentIntent.Id;
        payment.ErrorMessage = null;

        await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Stripe: payment {PaymentIntentId} completed for user {UserId}",
            paymentIntent.Id,
            payment.UserId);

        return ProcessStripeWebhookResult.Completed();
    }

    private async Task<ProcessStripeWebhookResult> HandleFailedPayment(
        Payment payment,
        StripePaymentIntentObject paymentIntent,
        CancellationToken cancellationToken)
    {
        payment.Status = PaymentStatus.Failed;
        payment.ErrorMessage = $"Платеж отклонен Stripe (статус: {paymentIntent.Status})";
        payment.ExternalPaymentId = paymentIntent.Id;

        await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);

        _logger.LogWarning("Stripe: payment {PaymentIntentId} failed for user {UserId}. Error: {Error}",
            paymentIntent.Id,
            payment.UserId,
            payment.ErrorMessage);

        return ProcessStripeWebhookResult.Completed("Платеж отклонен");
    }

    private async Task<ProcessStripeWebhookResult> HandleCancelledPayment(
        Payment payment,
        StripePaymentIntentObject paymentIntent,
        CancellationToken cancellationToken)
    {
        payment.Status = PaymentStatus.Cancelled;
        payment.ErrorMessage = "Платеж отменен";
        payment.ExternalPaymentId = paymentIntent.Id;

        await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Stripe: payment {PaymentIntentId} cancelled for user {UserId}",
            paymentIntent.Id,
            payment.UserId);

        return ProcessStripeWebhookResult.Completed("Платеж отменен");
    }
}

