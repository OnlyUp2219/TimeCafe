namespace Billing.TimeCafe.Application.CQRS.Payments.Commands;

public sealed record InitializeStripeCheckoutCommand(
    Guid UserId,
    decimal Amount,
    string? SuccessUrl,
    string? CancelUrl,
    string? Description) : ICommand<InitializeStripeCheckoutResponse>;

public sealed record InitializeStripeCheckoutResponse(
    Guid PaymentId,
    string SessionId,
    string CheckoutUrl);

public sealed class InitializeStripeCheckoutCommandValidator : AbstractValidator<InitializeStripeCheckoutCommand>
{
    public InitializeStripeCheckoutCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
        RuleFor(x => x.Amount).ValidPaymentAmount();
        RuleFor(x => x.SuccessUrl).ValidUrlWithPlaceholder("SuccessUrl");
        RuleFor(x => x.CancelUrl).ValidUrlWithPlaceholder("CancelUrl");
    }
}

public sealed class InitializeStripeCheckoutCommandHandler(
    IUnitOfWork uow,
    IStripePaymentClient stripeClient,
    IPublisher publisher,
    IOptionsSnapshot<StripeOptions> options,
    ILogger<InitializeStripeCheckoutCommandHandler> logger) : ICommandHandler<InitializeStripeCheckoutCommand, InitializeStripeCheckoutResponse>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IStripePaymentClient _stripeClient = stripeClient;
    private readonly IPublisher _publisher = publisher;
    private readonly IOptionsSnapshot<StripeOptions> _options = options;
    private readonly ILogger _logger = logger;

    public async Task<Result<InitializeStripeCheckoutResponse>> Handle(InitializeStripeCheckoutCommand request, CancellationToken cancellationToken = default)
    {
        var settings = _options.Value;
        var currency = settings.DefaultCurrency;

        var successUrl = string.IsNullOrWhiteSpace(request.SuccessUrl) ? settings.CheckoutSuccessUrl : request.SuccessUrl;
        var cancelUrl = string.IsNullOrWhiteSpace(request.CancelUrl) ? settings.CheckoutCancelUrl : request.CancelUrl;

        if (string.IsNullOrWhiteSpace(successUrl) || string.IsNullOrWhiteSpace(cancelUrl))
            return Result.Fail(new StripeWebhookError("Платежный провайдер не настроен (URL missing)"));

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? $"Пополнение баланса пользователя {request.UserId}"
            : request.Description;

        var paymentResult = Payment.Create(request.UserId, request.Amount);
        if (paymentResult.IsFailed)
        {
            return paymentResult.ToResult<InitializeStripeCheckoutResponse>();
        }

        var payment = paymentResult.Value;
        await _uow.Payments.CreateAsync(payment, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var createRequest = new StripeCreateCheckoutSessionRequest(
            payment.PaymentId,
            payment.UserId,
            payment.Amount,
            currency,
            description,
            successUrl,
            cancelUrl);

        var response = await _stripeClient.CreateCheckoutSessionAsync(createRequest, cancellationToken);

        if (!response.Success || string.IsNullOrWhiteSpace(response.SessionId))
        {
            _logger.LogError("Failed to create Stripe checkout session: {Error}", response.Error);
            payment.MarkAsFailed(response.Error ?? "Stripe returns error");
            await _uow.Payments.UpdateAsync(payment, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return Result.Fail(new StripeWebhookError(response.Error ?? "Unknown error"));
        }

        payment.ExternalPaymentId = response.SessionId;
        await _uow.Payments.UpdateAsync(payment, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new PaymentChangedEvent(payment.PaymentId, payment.UserId), cancellationToken);

        return Result.Ok(new InitializeStripeCheckoutResponse(payment.PaymentId, response.SessionId, response.CheckoutUrl!));
    }
}
