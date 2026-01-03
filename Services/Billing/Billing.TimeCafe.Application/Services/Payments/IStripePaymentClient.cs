namespace Billing.TimeCafe.Application.Services.Payments;

public interface IStripePaymentClient
{
    Task<StripeCreatePaymentResponse> CreatePaymentAsync(
        StripeCreatePaymentRequest request,
        CancellationToken ct = default);
}

public record StripeCreatePaymentRequest(
    Guid PaymentId,
    Guid UserId,
    decimal Amount,
    string Currency,
    string Description,
    string ReturnUrl);

public record StripeCreatePaymentResponse(
    bool Success,
    string? ExternalPaymentId = null,
    string? ClientSecret = null,
    string? PublishableKey = null,
    string? Error = null);
