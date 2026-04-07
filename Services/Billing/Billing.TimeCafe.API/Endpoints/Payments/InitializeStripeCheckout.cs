namespace Billing.TimeCafe.API.Endpoints.Payments;

public record InitializeCheckoutRequest(
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    Guid UserId,
    /// <example>1000.00</example>
    decimal Amount,
    /// <example>https://timecafe.ru/payment/success</example>
    string? SuccessUrl,
    /// <example>https://timecafe.ru/payment/cancel</example>
    string? CancelUrl,
    /// <example>Пополнение баланса</example>
    string? Description);

public class InitializeStripeCheckout : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/initialize-checkout", async (
            [FromServices] ISender sender,
            [FromBody] InitializeCheckoutRequest request) =>
        {
            var command = new InitializeStripeCheckoutCommand(
                request.UserId,
                request.Amount,
                request.SuccessUrl,
                request.CancelUrl,
                request.Description);

            var result = await sender.Send(command);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new
            {
                paymentId = r.PaymentId,
                sessionId = r.SessionId,
                checkoutUrl = r.CheckoutUrl
            }));
        })
        .WithTags("Payments")
        .WithName("InitializeStripeCheckout")
        .WithSummary("Инициализация платежа через Stripe Checkout")
        .WithDescription("Создаёт Stripe Checkout Session и возвращает URL для редиректа")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingPaymentInitialize));
    }
}
