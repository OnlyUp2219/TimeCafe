namespace Billing.TimeCafe.API.Endpoints.Admin.Transactions;

public class GetTransactionsPageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/transactions", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] Guid? userId,
            [FromServices] ISender sender) =>
        {
            var query = new GetTransactionsPageQuery(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, userId);
            var result = await sender.Send(query);
            return result.ToHttpResult(data => Results.Ok(new
            {
                transactions = data.Transactions,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount = data.TotalCount,
                    totalPages = data.TotalPages
                }
            }));
        })
        .WithTags("Admin - Billing")
        .WithName("GetTransactionsPage")
        .WithSummary("Получение страницы транзакций (admin)")
        .WithDescription("Возвращает страницу транзакций с опциональной фильтрацией по userId.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingAdminRead));
    }
}
