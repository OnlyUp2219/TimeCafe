namespace Billing.TimeCafe.API.Endpoints.Admin.Balances;

public class GetBalancesPageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/balances", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromServices] ISender sender) =>
        {
            var query = new GetBalancesPageQuery(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize);
            var result = await sender.Send(query);
            return result.ToHttpResult(data => Results.Ok(new
            {
                balances = data.Balances,
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
        .WithName("GetBalancesPage")
        .WithSummary("Получение страницы балансов (admin)")
        .WithDescription("Возвращает страницу балансов пользователей с пагинацией.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingAdminRead));
    }
}
