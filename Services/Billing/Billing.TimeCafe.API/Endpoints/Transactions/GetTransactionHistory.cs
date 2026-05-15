namespace Billing.TimeCafe.API.Endpoints.Transactions;

public class GetTransactionHistory : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/transactions/history/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new GetTransactionHistoryQuery(userId, page, pageSize);
            var result = await sender.Send(query);
            
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Transactions")
        .WithName("GetTransactionHistory")
        .WithSummary("История транзакций пользователя")
        .WithDescription("Возвращает историю транзакций с пагинацией (по 20 по умолчанию).")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingTransactionRead));
    }
}
