using Stripe;

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
}
