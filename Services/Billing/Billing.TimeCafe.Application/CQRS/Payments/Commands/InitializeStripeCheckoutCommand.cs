namespace Billing.TimeCafe.Application.CQRS.Payments.Commands;

public record InitializeStripeCheckoutCommand(
    string UserId,
    decimal Amount,
    string? SuccessUrl,
    string? CancelUrl,
    string? Description) : IRequest<InitializeStripeCheckoutResult>;

public record InitializeStripeCheckoutResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Guid? PaymentId = null,
    string? SessionId = null,
    string? CheckoutUrl = null) : ICqrsResultV2
{
    public static InitializeStripeCheckoutResult ConfigurationMissing() =>
        new(false, Code: "StripeConfigurationMissing", Message: "Платежный провайдер не настроен", StatusCode: 500);

    public static InitializeStripeCheckoutResult ProviderError(string message) =>
        new(false, Code: "StripeError", Message: message, StatusCode: 502);

    public static InitializeStripeCheckoutResult CheckoutCreated(Payment payment, string sessionId, string checkoutUrl) =>
        new(true,
            Message: "Checkout сессия создана",
            PaymentId: payment.PaymentId,
            SessionId: sessionId,
            CheckoutUrl: checkoutUrl);
}

public class InitializeStripeCheckoutCommandValidator : AbstractValidator<InitializeStripeCheckoutCommand>
{
    public InitializeStripeCheckoutCommandValidator()
    {
        RuleFor(x => x.UserId).ValidEntityId("Пользователь не найден");

        RuleFor(x => x.Amount).ValidPaymentAmount();

        RuleFor(x => x.SuccessUrl).ValidUrlWithPlaceholder("SuccessUrl");

        RuleFor(x => x.CancelUrl).ValidUrlWithPlaceholder("CancelUrl");
    }
}

public class InitializeStripeCheckoutCommandHandler(
    IPaymentRepository paymentRepository,
    IStripePaymentClient stripeClient,
    IOptions<StripeOptions> options,
    ILogger<InitializeStripeCheckoutCommandHandler> logger) : IRequestHandler<InitializeStripeCheckoutCommand, InitializeStripeCheckoutResult>
{
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly IStripePaymentClient _stripeClient = stripeClient;
    private readonly IOptions<StripeOptions> _options = options;
    private readonly ILogger _logger = logger;

    public async Task<InitializeStripeCheckoutResult> Handle(InitializeStripeCheckoutCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(request.UserId);

        var settings = _options.Value;
        var currency = settings.DefaultCurrency;

        var successUrl = string.IsNullOrWhiteSpace(request.SuccessUrl)
            ? settings.CheckoutSuccessUrl
            : request.SuccessUrl;

        var cancelUrl = string.IsNullOrWhiteSpace(request.CancelUrl)
            ? settings.CheckoutCancelUrl
            : request.CancelUrl;

        if (string.IsNullOrWhiteSpace(successUrl) || string.IsNullOrWhiteSpace(cancelUrl))
            return InitializeStripeCheckoutResult.ConfigurationMissing();

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? $"Пополнение баланса пользователя {userId}"
            : request.Description;

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            UserId = userId,
            Amount = request.Amount,
            PaymentMethod = PaymentMethod.Online,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _paymentRepository.CreateAsync(payment, cancellationToken);

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
            return InitializeStripeCheckoutResult.ProviderError(response.Error ?? "Unknown error");
        }

        payment.ExternalPaymentId = response.SessionId;
        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        return InitializeStripeCheckoutResult.CheckoutCreated(payment, response.SessionId, response.CheckoutUrl!);
    }
}
