namespace Billing.TimeCafe.API.Endpoints.Balance;

public class GetBalance : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/balance/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId) =>
        {
            var query = new GetBalanceQuery(userId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new
            {
                // TODO : Add Mapping
                balance = new
                {
                    r.Balance!.UserId,
                    r.Balance.CurrentBalance,
                    r.Balance.TotalDeposited,
                    r.Balance.TotalSpent,
                    r.Balance.Debt
                }
            }));
        })
        .WithTags("Balance")
        .WithName("GetBalance")
        .WithSummary("Получить баланс пользователя")
        .WithDescription("Возвращает текущий баланс, всего пополнено, всего потрачено и долг пользователя.")
        .Produces(200)
        .RequireAuthorization();
    }
}
