namespace Billing.TimeCafe.API.Endpoints.Payments;

public record InitializeCheckoutRequest(Guid UserId, decimal Amount, string? SuccessUrl, string? CancelUrl, string? Description);

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
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new
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
        .WithOpenApi()
        .RequireAuthorization();
    }
}
