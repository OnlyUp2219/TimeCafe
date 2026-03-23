namespace Billing.TimeCafe.API.Endpoints.Payments;

public record InitializePaymentRequest(Guid UserId, decimal Amount, string? ReturnUrl, string? Description);

public class InitializeStripePayment : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/initialize", async (
            [FromServices] ISender sender,
            [FromBody] InitializePaymentRequest request) =>
        {
            var command = new InitializeStripePaymentCommand(
                request.UserId,
                request.Amount,
                request.ReturnUrl,
                request.Description);

            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new
            {
                paymentId = r.PaymentId,
                externalPaymentId = r.ExternalPaymentId,
                clientSecret = r.ClientSecret,
                publishableKey = r.PublishableKey
            }));
        })
        .WithTags("Payments")
        .WithName("InitializeStripePayment")
        .WithSummary("Инициализация платежа через Stripe")
        .WithDescription("Создаёт платёж Stripe и возвращает client secret для фронта")
        .WithOpenApi()
        .RequireAuthorization();
    }
}
