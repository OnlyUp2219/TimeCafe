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
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Сумма должна быть больше нуля")
            .GreaterThanOrEqualTo(50m).WithMessage("Минимальная сумма платежа 50 ₽ (требование Stripe)");

        RuleFor(x => x.SuccessUrl)
            .Must(IsValidUrlWithPlaceholder)
            .WithMessage("SuccessUrl некорректен");

        RuleFor(x => x.CancelUrl)
            .Must(IsValidUrlWithPlaceholder)
            .WithMessage("CancelUrl некорректен");
    }

    private static bool IsValidUrlWithPlaceholder(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        var urlToValidate = url.Replace("{CHECKOUT_SESSION_ID}", "placeholder");
        return Uri.IsWellFormedUriString(urlToValidate, UriKind.Absolute);
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

        _logger.LogInformation("Initializing Stripe checkout: Amount={Amount}, Currency={Currency}, UserId={UserId}", 
            request.Amount, currency, userId);

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

        await _paymentRepository.CreateAsync(payment, cancellationToken).ConfigureAwait(false);

        var createRequest = new StripeCreateCheckoutSessionRequest(
            payment.PaymentId,
            payment.UserId,
            payment.Amount,
            currency,
            description,
            successUrl,
            cancelUrl);

        var response = await _stripeClient.CreateCheckoutSessionAsync(createRequest, cancellationToken).ConfigureAwait(false);

        if (!response.Success || string.IsNullOrWhiteSpace(response.SessionId))
        {
            _logger.LogError("Failed to create Stripe checkout session: {Error}", response.Error);
            return InitializeStripeCheckoutResult.ProviderError(response.Error ?? "Unknown error");
        }

        payment.ExternalPaymentId = response.SessionId;
        await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Checkout session created: {SessionId} for user {UserId}", response.SessionId, userId);

        return InitializeStripeCheckoutResult.CheckoutCreated(payment, response.SessionId, response.CheckoutUrl!);
    }
}
