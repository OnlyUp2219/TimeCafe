namespace Billing.TimeCafe.API.Endpoints.Balance;

public class GetBalancesBulk : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/balance/bulk", async (
            [FromServices] ISender sender,
            [FromBody] IEnumerable<Guid> userIds) =>
        {
            var query = new GetBalancesBulkQuery(userIds);
            var result = await sender.Send(query);
            
            return result.ToHttpResult(onSuccess: r => TypedResults.Ok(r));
        })
        .WithTags("Balance")
        .WithName("GetBalancesBulk")
        .WithSummary("Получить балансы нескольких пользователей")
        .WithDescription("Возвращает словарь с балансами запрошенных пользователей.")
        .Produces<IDictionary<Guid, decimal>>(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingBalanceRead));
    }
}
