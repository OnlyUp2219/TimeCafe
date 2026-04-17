namespace Venue.TimeCafe.API.Endpoints.Tariffs.Queries;

public class GetTariffsPage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tariffs/page", async (
            [FromServices] ISender sender,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new GetTariffsPageQuery(pageNumber <= 0 ? 1 : pageNumber, pageSize <= 0 ? 20 : pageSize);
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { tariffs = r.Tariffs, totalCount = r.TotalCount }));
        })
        .WithTags("Tariffs")
        .WithName("GetTariffsPage")
        .WithSummary("Получить страницу тарифов")
        .WithDescription("Возвращает страницу тарифов с пагинацией.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueTariffRead));
    }
}
