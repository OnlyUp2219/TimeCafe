namespace Billing.TimeCafe.Application.CQRS.Payments.Commands;

public record InitializeStripePaymentCommand(
    string UserId,
    decimal Amount,
    string? ReturnUrl,
    string? Description) : IRequest<InitializeStripePaymentResult>;

public record InitializeStripePaymentResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Guid? PaymentId = null,
    string? ExternalPaymentId = null,
    string? ClientSecret = null,
    string? PublishableKey = null) : ICqrsResultV2
{
    public static InitializeStripePaymentResult ConfigurationMissing() =>
        new(false, Code: "StripeConfigurationMissing", Message: "Платежный провайдер не настроен", StatusCode: 500);

    public static InitializeStripePaymentResult ProviderError(string message) =>
        new(false, Code: "StripeError", Message: message, StatusCode: 502);

    public static InitializeStripePaymentResult PaymentCreated(Payment payment, string? clientSecret, string? publishableKey) =>
        new(true,
            Message: "Платёж инициализирован",
            PaymentId: payment.PaymentId,
            ExternalPaymentId: payment.ExternalPaymentId,
            ClientSecret: clientSecret,
            PublishableKey: publishableKey);
}

public class InitializeStripePaymentCommandValidator : AbstractValidator<InitializeStripePaymentCommand>
{
    public InitializeStripePaymentCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Сумма должна быть больше нуля")
            .GreaterThanOrEqualTo(50).WithMessage("Минимальная сумма платежа 50 рублей");

        RuleFor(x => x.ReturnUrl)
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("ReturnUrl некорректен");
    }
}

public class InitializeStripePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IStripePaymentClient stripeClient,
    IOptions<StripeOptions> options,
    ILogger<InitializeStripePaymentCommandHandler> logger) : IRequestHandler<InitializeStripePaymentCommand, InitializeStripePaymentResult>
{
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly IStripePaymentClient _stripeClient = stripeClient;
    private readonly IOptions<StripeOptions> _options = options;
    private readonly ILogger _logger = logger;

    public async Task<InitializeStripePaymentResult> Handle(InitializeStripePaymentCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(request.UserId);

        var settings = _options.Value;
        var currency = string.IsNullOrWhiteSpace(settings.DefaultCurrency) ? "rub" : settings.DefaultCurrency;

        var returnUrl = string.IsNullOrWhiteSpace(request.ReturnUrl)
            ? settings.DefaultReturnUrl
            : request.ReturnUrl;

        if (string.IsNullOrWhiteSpace(returnUrl))
            return InitializeStripePaymentResult.ConfigurationMissing();

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

        var createRequest = new StripeCreatePaymentRequest(
            payment.PaymentId,
            payment.UserId,
            payment.Amount,
            currency,
            description,
            returnUrl);

        var providerResponse = await _stripeClient.CreatePaymentAsync(createRequest, cancellationToken).ConfigureAwait(false);

        if (!providerResponse.Success)
        {
            payment.Status = PaymentStatus.Failed;
            payment.ErrorMessage = providerResponse.Error;
            await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);
            return InitializeStripePaymentResult.ProviderError(providerResponse.Error ?? "Stripe возвращает ошибку");
        }

        payment.ExternalPaymentId = providerResponse.ExternalPaymentId;
        payment.Status = PaymentStatus.Pending;

        await _paymentRepository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Stripe: created payment intent {PaymentIntentId} for user {UserId}",
            payment.ExternalPaymentId,
            payment.UserId);

        return InitializeStripePaymentResult.PaymentCreated(payment, providerResponse.ClientSecret, providerResponse.PublishableKey);
    }
}
