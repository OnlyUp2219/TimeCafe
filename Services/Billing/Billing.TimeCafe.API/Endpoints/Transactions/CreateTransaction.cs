using Billing.TimeCafe.Application.CQRS.Balances.Commands;
using BuildingBlocks.Extensions;

namespace Billing.TimeCafe.API.Endpoints.Transactions;

public record CreateTransactionRequest(
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    Guid UserId,
    /// <example>500.00</example>
    decimal Amount,
    /// <example>Deposit</example>
    TransactionType Type,
    /// <example>Manual</example>
    TransactionSource Source,
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    Guid? SourceId,
    /// <example>Пополнение баланса</example>
    string? Comment);

public class CreateTransaction : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/transactions", async (
            [FromServices] ISender sender,
            [FromBody] CreateTransactionRequest request) =>
        {
            var command = new AdjustBalanceCommand(
                request.UserId,
                request.Amount,
                request.Type,
                request.Source,
                request.SourceId,
                request.Comment);

            var result = await sender.Send(command);
            
            return result.ToHttpResult(onSuccess: r => TypedResults.Ok(new
            {
                balance = new
                {
                    userId = r.UserId,
                    amount = r.CurrentBalance
                },
                transaction = new
                {
                    id = r.TransactionId,
                    amount = r.TransactionAmount,
                    type = r.TransactionType
                }
            }));
        })
        .WithTags("Transactions")
        .WithName("CreateTransaction")
        .WithSummary("Создать транзакцию (пополнение/списание)")
        .WithDescription("Создаёт транзакцию и корректирует баланс пользователя")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingTransactionCreate));
    }
}
