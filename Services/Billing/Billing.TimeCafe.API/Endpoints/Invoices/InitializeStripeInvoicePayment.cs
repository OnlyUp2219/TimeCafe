namespace Billing.TimeCafe.API.Endpoints.Invoices;

public record InitializeStripeInvoicePaymentRequest(string SuccessUrl, string CancelUrl);

public class InitializeStripeInvoicePayment : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/invoices/{invoiceId:guid}/pay-stripe", async (
            [FromServices] ISender sender,
            [FromRoute] Guid invoiceId,
            [FromBody] InitializeStripeInvoicePaymentRequest request) =>
        {
            var command = new InitializeStripeInvoicePaymentCommand(invoiceId, request.SuccessUrl, request.CancelUrl);
            var result = await sender.Send(command);

            return result.ToHttpResult(onSuccess: TypedResults.Ok);
        })
        .WithTags("Invoices")
        .WithName("InitializeStripeInvoicePayment")
        .WithSummary("Инициализировать Stripe Checkout для оплаты счёта")
        .WithDescription("Создаёт сессию Stripe Checkout для оплаты счёта и возвращает URL для перенаправления.")
        .Produces<InitializeStripeInvoicePaymentResponse>(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingInvoicePay));
    }
}
