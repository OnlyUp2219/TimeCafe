namespace Billing.TimeCafe.API.Endpoints.Transactions;

public class GetTransactionHistory : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/billing/transactions/history/{userId}", async (
            [FromServices] ISender sender,
            [AsParameters] GetTransactionHistoryDto dto) =>
        {
            var query = new GetTransactionHistoryQuery(dto.UserId, dto.Page, dto.PageSize);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new
            {
                transactions = r.Transactions,
                pagination = new
                {
                    currentPage = dto.Page,
                    pageSize = dto.PageSize,
                    totalCount = r.TotalCount,
                    totalPages = r.TotalPages
                }
            }));
        })
        .WithTags("Transactions")
        .WithName("GetTransactionHistory")
        .WithSummary("История транзакций пользователя")
        .WithDescription("Возвращает историю транзакций с пагинацией (по 10 по умолчанию, макс 100).")
        .WithOpenApi()
        .RequireAuthorization();
    }
}
