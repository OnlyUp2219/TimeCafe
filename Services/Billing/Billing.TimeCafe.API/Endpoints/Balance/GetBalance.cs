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
            
            return result.ToHttpResult(onSuccess: r => TypedResults.Ok(new
            {
                balance = new
                {
                    userId = r.UserId,
                    amount = r.CurrentBalance,
                    debt = r.Debt,
                    lastUpdated = r.LastUpdated
                }
            }));
        })
        .WithTags("Balance")
        .WithName("GetBalance")
        .WithSummary("Получить баланс пользователя")
        .WithDescription("Возвращает текущий баланс, всего пополнено, всего потрачено и долг пользователя.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingBalanceRead));
    }
}
