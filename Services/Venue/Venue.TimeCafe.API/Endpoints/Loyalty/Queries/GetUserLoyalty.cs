namespace Venue.TimeCafe.API.Endpoints.Loyalty.Queries;

public class GetUserLoyalty : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/loyalty/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId) =>
        {
            var query = new GetUserLoyaltyQuery(userId);
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Loyalty")
        .WithName("GetUserLoyalty")
        .WithSummary("Получить уровень лояльности пользователя")
        .WithDescription("Возвращает размер персональной скидки для указанного пользователя.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueLoyaltyRead));
    }
}
