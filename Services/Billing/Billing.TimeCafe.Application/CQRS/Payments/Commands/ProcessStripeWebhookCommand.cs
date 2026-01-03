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

        RuleFor(x => x.Payload.Data)
            .NotNull().WithMessage("Платёж не найден")
            .When(x => x.Payload is not null);

        RuleFor(x => x.Payload.Data.Object.Id)
            .NotEmpty().WithMessage("Платёж не найден")
            .When(x => x.Payload?.Data?.Object is not null);
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
        var payload = request.Payload;

        var webhookSecret = _options.Value.WebhookSecret;
        if (!string.IsNullOrWhiteSpace(webhookSecret) && string.IsNullOrWhiteSpace(request.SignatureHeader))
        {
            _logger.LogWarning("Stripe webhook: missing signature header");
            return ProcessStripeWebhookResult.Unauthorized();
        }

        if (payload?.Data?.Object == null || string.IsNullOrWhiteSpace(payload.Data.Object.Id))
            return ProcessStripeWebhookResult.PaymentNotFound();

        var paymentIntentId = payload.Data.Object.Id;
        var payment = await _paymentRepository.GetByExternalPaymentIdAsync(paymentIntentId, cancellationToken)
            .ConfigureAwait(false);

        if (payment == null && payload.Data.Object.Metadata?.TryGetValue("paymentId", out var paymentIdStr) == true)
        {
            if (Guid.TryParse(paymentIdStr, out var paymentId))
            {
                payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken).ConfigureAwait(false);
            }
        }

        if (payment == null)
            return ProcessStripeWebhookResult.PaymentNotFound();

        if (payment.Status == PaymentStatus.Completed)
            return ProcessStripeWebhookResult.AlreadyProcessed();

        if (string.Equals(payload.Type, "payment_intent.succeeded", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleSuccessfulPayment(payment, payload, cancellationToken);
        }

        if (string.Equals(payload.Type, "payment_intent.payment_failed", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleFailedPayment(payment, payload, cancellationToken);
        }

        if (string.Equals(payload.Type, "payment_intent.canceled", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleCancelledPayment(payment, payload, cancellationToken);
        }

        _logger.LogInformation("Stripe: event {Event} for payment {PaymentId} skipped", payload.Type, payment.PaymentId);
        return ProcessStripeWebhookResult.Ignored();
    }

    private async Task<ProcessStripeWebhookResult> HandleSuccessfulPayment(
        Payment payment,
        StripeWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        var amountFromStripe = payload.Data.Object.Amount / 100m;

        if (Math.Abs(amountFromStripe - payment.Amount) > 0.01m)
        {
            _logger.LogWarning("Stripe: amount mismatch {WebhookAmount} vs {Expected}", amountFromStripe, payment.Amount);
        }

        var adjustResult = await _sender.Send(new AdjustBalanceCommand(
            payment.UserId,
            amountFromStripe,
            TransactionType.Deposit,
            TransactionSource.Payment,
            payment.PaymentId,
            "Пополнение баланса через Stripe"), cancellationToken).ConfigureAwait(false);

        if (!adjustResult.Success)
        {
            payment.Status = PaymentStatus.Failed;
            payment.ErrorMessage = adjustResult.Message ?? "Не удалось зачислить оплату";
            await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);
            return ProcessStripeWebhookResult.ProviderError(adjustResult.Message ?? "Ошибка зачисления платежа");
        }

        payment.Status = PaymentStatus.Completed;
        payment.CompletedAt = DateTimeOffset.FromUnixTimeSeconds(payload.Data.Object.Created);
        payment.TransactionId = adjustResult.Transaction?.TransactionId;
        payment.ExternalPaymentId = payload.Data.Object.Id;
        payment.ErrorMessage = null;

        await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Stripe: payment {PaymentIntentId} completed for user {UserId}",
            payload.Data.Object.Id,
            payment.UserId);

        return ProcessStripeWebhookResult.Completed();
    }

    private async Task<ProcessStripeWebhookResult> HandleFailedPayment(
        Payment payment,
        StripeWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        payment.Status = PaymentStatus.Failed;
        payment.ErrorMessage = $"Платеж отклонен Stripe (статус: {payload.Data.Object.Status})";
        payment.ExternalPaymentId = payload.Data.Object.Id;

        await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);

        _logger.LogWarning("Stripe: payment {PaymentIntentId} failed for user {UserId}. Error: {Error}",
            payload.Data.Object.Id,
            payment.UserId,
            payment.ErrorMessage);

        return ProcessStripeWebhookResult.Completed("Платеж отклонен");
    }

    private async Task<ProcessStripeWebhookResult> HandleCancelledPayment(
        Payment payment,
        StripeWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        payment.Status = PaymentStatus.Cancelled;
        payment.ErrorMessage = "Платеж отменен";
        payment.ExternalPaymentId = payload.Data.Object.Id;

        await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Stripe: payment {PaymentIntentId} cancelled for user {UserId}",
            payload.Data.Object.Id,
            payment.UserId);

        return ProcessStripeWebhookResult.Completed("Платеж отменен");
    }
}

