namespace Billing.TimeCafe.API.Endpoints.Transactions;

public class GetTransaction : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/transactions/{transactionId}", async (
            [FromServices] ISender sender,
            [AsParameters] GetTransactionDto dto) =>
        {
            var query = new GetTransactionByIdQuery(dto.TransactionId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { transaction = r.Transaction }));
        })
        .WithTags("Transactions")
        .WithName("GetTransaction")
        .WithSummary("Получить транзакцию по ID")
        .WithDescription("Возвращает информацию о конкретной транзакции.")
        .WithOpenApi()
        .RequireAuthorization();
    }
}
