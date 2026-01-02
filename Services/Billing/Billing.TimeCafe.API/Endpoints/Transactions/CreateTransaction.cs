namespace Billing.TimeCafe.API.Endpoints.Transactions;

using Billing.TimeCafe.API.DTOs;

public class CreateTransaction : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/billing/transactions", async (
            [FromServices] ISender sender,
            [FromBody] AdjustBalanceDto dto) =>
        {
            var command = new AdjustBalanceCommand(
                dto.UserId,
                dto.Amount,
                (TransactionType)dto.Type,
                (TransactionSource)dto.Source,
                dto.SourceId,
                dto.Comment);

            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new
            {
                balance = new
                {
                    r.Balance!.UserId,
                    r.Balance!.CurrentBalance,
                    r.Balance!.TotalDeposited,
                    r.Balance!.TotalSpent,
                    r.Balance!.Debt
                },
                transaction = new
                {
                    r.Transaction!.TransactionId,
                    r.Transaction!.UserId,
                    r.Transaction!.Amount,
                    r.Transaction!.Type,
                    r.Transaction!.Source,
                    r.Transaction!.BalanceAfter,
                    r.Transaction!.CreatedAt
                }
            }));
        })
        .WithTags("Transactions")
        .WithName("CreateTransaction")
        .WithSummary("Создать транзакцию (пополнение/списание)")
        .WithDescription("Создаёт транзакцию и корректирует баланс пользователя")
        .WithOpenApi()
        .RequireAuthorization();
    }
}
