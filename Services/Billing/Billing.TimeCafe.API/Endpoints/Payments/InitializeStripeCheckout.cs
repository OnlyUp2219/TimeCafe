namespace Billing.TimeCafe.API.Endpoints.Payments;

public class InitializeStripeCheckout : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/billing/payments/initialize-checkout", async (
            [FromServices] ISender sender,
            [FromBody] InitializeCheckoutDto dto) =>
        {
            var command = new InitializeStripeCheckoutCommand(
                dto.UserId,
                dto.Amount,
                dto.SuccessUrl,
                dto.CancelUrl,
                dto.Description);

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
