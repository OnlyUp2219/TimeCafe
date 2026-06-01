namespace Billing.TimeCafe.API.Endpoints.Invoices;

public record PayInvoiceRequest(PaymentMethod Method);

public class PayInvoice : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/invoices/{invoiceId:guid}/pay", async (
            [FromServices] ISender sender,
            [FromRoute] Guid invoiceId,
            [FromBody] PayInvoiceRequest request) =>
        {
            var command = new PayInvoiceCommand(invoiceId, request.Method);
            var result = await sender.Send(command);

            return result.ToHttpResult(onSuccess: () => TypedResults.Ok(new { message = "Счёт успешно оплачен" }));
        })
        .WithTags("Invoices")
        .WithName("PayInvoice")
        .WithSummary("Оплатить счёт")
        .WithDescription("Позволяет оплатить счёт с внутреннего баланса (клиентом) либо наличными/картой (администратором).")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(
            Permissions.BillingInvoicePay, 
            Permissions.BillingInvoiceAdminWrite));
    }
}
