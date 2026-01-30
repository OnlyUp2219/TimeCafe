namespace Billing.TimeCafe.API.Endpoints.Balance;

using Billing.TimeCafe.API.DTOs;

public class GetUserDebt : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/debt/{userId}", async (
            [FromServices] ISender sender,
            [AsParameters] GetUserDebtDto dto) =>
        {
            var query = new GetUserDebtQuery(dto.UserId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { debt = r.Debt }));
        })
        .WithTags("Balance")
        .WithName("GetUserDebt")
        .WithSummary("Получить долг пользователя")
        .WithDescription("Возвращает сумму задолженности пользователя.")
        .WithOpenApi()
        .RequireAuthorization();
    }
}
