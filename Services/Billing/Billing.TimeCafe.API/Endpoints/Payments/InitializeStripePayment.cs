namespace Billing.TimeCafe.API.Endpoints.Payments;

public record InitializePaymentRequest(
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    Guid UserId,
    /// <example>1000.00</example>
    decimal Amount,
    /// <example>https://timecafe.ru/payment/success</example>
    string? ReturnUrl,
    /// <example>Пополнение баланса</example>
    string? Description);

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
            return result.ToHttpResult(onSuccess: r => Results.Ok(new
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
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingPaymentInitialize));
    }
}
