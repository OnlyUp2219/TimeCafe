namespace Venue.TimeCafe.API.Endpoints.Tariffs.Queries;

public class GetTariffsPage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tariffs/page", async (
            [FromServices] ISender sender,
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize) =>
        {
            var query = new GetTariffsPageQuery(pageNumber, pageSize);
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { tariffs = r.Tariffs, totalCount = r.TotalCount }));
        })
        .WithTags("Tariffs")
        .WithName("GetTariffsPage")
        .WithSummary("Получить страницу тарифов")
        .WithDescription("Возвращает страницу тарифов с пагинацией.")
        .Produces(200)
        .RequireAuthorization();
    }
}
