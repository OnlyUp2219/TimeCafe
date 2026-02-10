using Stripe;
using Stripe.Checkout;

namespace Billing.TimeCafe.Infrastructure.Services.Stripe;

public class StripePaymentClient : IStripePaymentClient
{
    private readonly IOptions<StripeOptions> _options;
    private readonly ILogger<StripePaymentClient> _logger;

    public StripePaymentClient(IOptions<StripeOptions> options, ILogger<StripePaymentClient> logger)
    {
        _options = options;
        _logger = logger;

        var secretKey = options.Value.SecretKey;
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException("Stripe:SecretKey is not configured.");

        StripeConfiguration.ApiKey = secretKey;
    }

    public async Task<StripeCreatePaymentResponse> CreatePaymentAsync(
        StripeCreatePaymentRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                // TODO: Получать актуальный курс валюты (сейчас 100 центов = 1 рубль)
                // В продакшене нужна интеграция с API курсов валют (например, API ЦБ РФ/РБ)
                Amount = (long)(request.Amount * 100),
                Currency = string.IsNullOrWhiteSpace(request.Currency) ? "rub" : request.Currency,
                Description = request.Description,
                Metadata = new Dictionary<string, string>
                {
                    { "paymentId", request.PaymentId.ToString() },
                    { "userId", request.UserId.ToString() }
                },
                StatementDescriptorSuffix = "TimeCafe"
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, null, ct);

            if (paymentIntent == null)
                return new StripeCreatePaymentResponse(false, Error: "Failed to create payment intent");

            _logger.LogInformation("Stripe: created payment intent {PaymentIntentId} for user {UserId}",
                paymentIntent.Id, request.UserId);

            return new StripeCreatePaymentResponse(
                true,
                paymentIntent.Id,
                paymentIntent.ClientSecret,
                _options.Value.PublishableKey);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error: {Message}", ex.Message);
            return new StripeCreatePaymentResponse(false, Error: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating Stripe payment");
            return new StripeCreatePaymentResponse(false, Error: "Unexpected error");
        }
    }

    public async Task<StripeCreateCheckoutSessionResponse> CreateCheckoutSessionAsync(
        StripeCreateCheckoutSessionRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                Mode = "payment",
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "rub" : request.Currency,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Пополнение баланса TimeCafe",
                                Description = request.Description
                            },
                            UnitAmount = (long)(request.Amount * 100)
                        },
                        Quantity = 1
                    }
                },
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "paymentId", request.PaymentId.ToString() },
                    { "userId", request.UserId.ToString() }
                },
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "paymentId", request.PaymentId.ToString() },
                        { "userId", request.UserId.ToString() }
                    }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options, null, ct);

            if (session == null)
                return new StripeCreateCheckoutSessionResponse(false, Error: "Failed to create checkout session");

            _logger.LogInformation("Stripe: created checkout session {SessionId} for user {UserId}",
                session.Id, request.UserId);

            return new StripeCreateCheckoutSessionResponse(
                true,
                session.Id,
                session.Url);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error: {Message}", ex.Message);
            return new StripeCreateCheckoutSessionResponse(false, Error: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating Stripe checkout session");
            return new StripeCreateCheckoutSessionResponse(false, Error: "Unexpected error");
        }
    }
}
