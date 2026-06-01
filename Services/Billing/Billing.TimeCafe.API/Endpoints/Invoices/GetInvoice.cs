namespace Billing.TimeCafe.API.Endpoints.Invoices;

public class GetInvoice : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/invoices/{invoiceId:guid}", async (
            [FromServices] ISender sender,
            [FromRoute] Guid invoiceId) =>
        {
            var query = new GetInvoiceByIdQuery(invoiceId);
            var result = await sender.Send(query);

            return result.ToHttpResult(onSuccess: TypedResults.Ok);
        })
        .WithTags("Invoices")
        .WithName("GetInvoiceById")
        .WithSummary("Получить счёт по идентификатору")
        .WithDescription("Возвращает детальную информацию о счёте на оплату.")
        .Produces<Invoice>(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(
            Permissions.BillingInvoiceRead, 
            Permissions.BillingInvoiceAdminRead));

        app.MapGet("/invoices/visit/{visitId:guid}", async (
            [FromServices] ISender sender,
            [FromRoute] Guid visitId) =>
        {
            var query = new GetInvoiceByVisitIdQuery(visitId);
            var result = await sender.Send(query);

            return result.ToHttpResult(onSuccess: TypedResults.Ok);
        })
        .WithTags("Invoices")
        .WithName("GetInvoiceByVisitId")
        .WithSummary("Получить счёт по идентификатору визита")
        .WithDescription("Возвращает детальную информацию о счёте по идентификатору визита.")
        .Produces<Invoice>(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(
            Permissions.BillingInvoiceRead, 
            Permissions.BillingInvoiceAdminRead));
    }
}
