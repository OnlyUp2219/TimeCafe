using Billing.TimeCafe.Application.CQRS.Transactions.Queries;

namespace Billing.TimeCafe.API.Endpoints.Admin.Transactions;

public sealed record GetTransactionsPageRequest(int Page = 1, int PageSize = 20, Guid? UserId = null);

public class GetTransactionsPageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/billing/admin")
            .WithTags("Admin - Billing")
            .MapGet("/transactions", async (
                [AsParameters] GetTransactionsPageRequest request,
                [FromServices] ISender sender) =>
            {
                var query = new GetTransactionsPageQuery(request.Page, request.PageSize, request.UserId);
                var result = await sender.Send(query);
                return result.ToHttpResult(data => Results.Ok(new
                {
                    transactions = data.Transactions,
                    pagination = new
                    {
                        currentPage = request.Page,
                        pageSize = request.PageSize,
                        totalCount = data.TotalCount,
                        totalPages = data.TotalPages
                    }
                }));
            })
            .WithName("GetTransactionsPage")
            .WithSummary("Получение страницы транзакций (admin)")
            .WithDescription("Возвращает страницу транзакций с опциональной фильтрацией по userId.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingAdminRead));
    }
}
