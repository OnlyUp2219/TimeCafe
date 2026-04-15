using Billing.TimeCafe.Application.CQRS.Balances.Queries;

namespace Billing.TimeCafe.API.Endpoints.Admin.Balances;

public sealed record GetBalancesPageRequest(int Page = 1, int PageSize = 20);

public class GetBalancesPageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/billing/admin")
            .WithTags("Admin - Billing")
            .MapGet("/balances", async (
                [AsParameters] GetBalancesPageRequest request,
                [FromServices] ISender sender) =>
            {
                var query = new GetBalancesPageQuery(request.Page, request.PageSize);
                var result = await sender.Send(query);
                return result.ToHttpResult(data => Results.Ok(new
                {
                    balances = data.Balances,
                    pagination = new
                    {
                        currentPage = request.Page,
                        pageSize = request.PageSize,
                        totalCount = data.TotalCount,
                        totalPages = data.TotalPages
                    }
                }));
            })
            .WithName("GetBalancesPage")
            .WithSummary("Получение страницы балансов (admin)")
            .WithDescription("Возвращает страницу балансов пользователей с пагинацией.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingAdminRead));
    }
}
