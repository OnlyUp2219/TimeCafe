namespace Venue.TimeCafe.API.Endpoints.Tariffs.Queries;

public class GetTariffsPage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tariffs/page", async (
            [FromServices] ISender sender,
            [FromBody] GetTariffsPageDto dto) =>
        {
            // TODO : DTO
            var query = new GetTariffsPageQuery(dto.PageNumber, dto.PageSize);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { tariffs = r.Tariffs, totalCount = r.TotalCount }));
        })
        .WithTags("Tariffs")
        .WithName("GetTariffsPage")
        .WithSummary("Получить страницу тарифов")
        .WithDescription("Возвращает страницу тарифов с пагинацией.");
    }
}
