namespace Billing.TimeCafe.API.Endpoints.Transactions;

public class GetTransactionHistory : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/transactions/history/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10) =>
        {
            var query = new GetTransactionHistoryQuery(userId, page, pageSize);
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new
            {
                transactions = r.Transactions,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = r.TotalCount,
                    totalPages = r.TotalPages
                }
            }));
        })
        .WithTags("Transactions")
        .WithName("GetTransactionHistory")
        .WithSummary("История транзакций пользователя")
        .WithDescription("Возвращает историю транзакций с пагинацией (по 10 по умолчанию, макс 100).")
        .Produces(200)
        .RequireAuthorization();
    }
}
