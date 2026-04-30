namespace Venue.TimeCafe.API.Endpoints.Promotions.Queries;

public class GetPromotionsPage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/promotions/page", async (
            [FromServices] ISender sender,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new GetPromotionsPageQuery(pageNumber <= 0 ? 1 : pageNumber, pageSize <= 0 ? 20 : pageSize);
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Promotions")
        .WithName("GetPromotionsPage")
        .WithSummary("Получить страницу акций")
        .WithDescription("Возвращает страницу акций с пагинацией.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenuePromotionRead));
    }
}
