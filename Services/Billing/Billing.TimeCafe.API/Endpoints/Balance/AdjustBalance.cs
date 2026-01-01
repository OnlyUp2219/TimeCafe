namespace Billing.TimeCafe.API.Endpoints.Balance;

using Billing.TimeCafe.API.DTOs;

public class AdjustBalance : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/billing/balance/adjust", async (
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
                    r.Balance.UserId,
                    r.Balance.CurrentBalance,
                    r.Balance.TotalDeposited,
                    r.Balance.TotalSpent,
                    r.Balance.Debt
                },
                transaction = new
                {
                    r.Transaction.TransactionId,
                    r.Transaction.UserId,
                    r.Transaction.Amount,
                    r.Transaction.Type,
                    r.Transaction.Source,
                    r.Transaction.BalanceAfter,
                    r.Transaction.CreatedAt
                }
            }));
        })
        .WithTags("Balance")
        .WithName("AdjustBalance")
        .WithSummary("Корректировать баланс")
        .WithDescription("Пополняет или списывает со счета. Создает транзакцию для учета.")
        .WithOpenApi()
        .RequireAuthorization();
    }
}
