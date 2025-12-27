namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public class GetAdditionalInfoById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/infos/{infoId}", async (
            [FromServices] ISender sender,
            [AsParameters] GetAdditionalInfoByIdDto dto) =>
        {
            var query = new GetAdditionalInfoByIdQuery(dto.InfoId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(r.AdditionalInfo));
        })
        .WithTags("AdditionalInfos")
        .WithName("GetAdditionalInfoById")
        .WithSummary("Получить доп. информацию по Id")
        .WithDescription("Возвращает запись дополнительной информации по идентификатору.")
        .RequireAuthorization();
    }
}
