namespace Billing.TimeCafe.API.Endpoints.Admin.Invoices;

public class GetInvoicesPage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/invoices", async (
            [FromServices] ISender sender,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Guid? userId = null) =>
        {
            var query = new GetInvoicesPageQuery(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, userId);
            var result = await sender.Send(query);

            return result.ToHttpResult(onSuccess: r => TypedResults.Ok(new
            {
                invoices = r.Invoices,
                pagination = new
                {
                    currentPage = page <= 0 ? 1 : page,
                    pageSize = pageSize <= 0 ? 20 : pageSize,
                    totalCount = r.TotalCount,
                    totalPages = r.TotalPages
                }
            }));
        })
        .WithTags("Admin Invoices")
        .WithName("GetInvoicesPage")
        .WithSummary("Получить список счетов (админ)")
        .WithDescription("Возвращает список всех счетов с возможностью фильтрации по пользователю и пагинацией.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingInvoiceAdminRead));
    }
}
