namespace Billing.TimeCafe.API.Endpoints.Admin.Transactions;

public class GetTransactionsPageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/transactions", async (
            [FromServices] ISender sender,
            [FromQuery] Guid? userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new GetTransactionsPageQuery(page, pageSize, userId);
            var result = await sender.Send(query);
            
            return result.ToHttpResult(data => TypedResults.Ok(data));
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
