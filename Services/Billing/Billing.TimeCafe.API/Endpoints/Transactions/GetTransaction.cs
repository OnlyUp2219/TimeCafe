namespace Billing.TimeCafe.API.Endpoints.Transactions;

public class GetTransaction : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/transactions/{transactionId:guid}", async (
            [FromServices] ISender sender,
            Guid transactionId) =>
        {
            var query = new GetTransactionByIdQuery(transactionId);
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { transaction = r.Transaction }));
        })
        .WithTags("Transactions")
        .WithName("GetTransaction")
        .WithSummary("Получить транзакцию по ID")
        .WithDescription("Возвращает информацию о конкретной транзакции.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization();
    }
}
