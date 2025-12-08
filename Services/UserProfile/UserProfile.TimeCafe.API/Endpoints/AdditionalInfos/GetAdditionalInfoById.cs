namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public class GetAdditionalInfoById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/infos/{infoId:guid}", async (
            ISender sender,
            Guid infoId) =>
        {
            var query = new GetAdditionalInfoByIdQuery(infoId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(r.AdditionalInfo));
        })
        .WithTags("AdditionalInfos")
        .WithName("GetAdditionalInfoById")
        .WithSummary("Получить доп. информацию по Id")
        .WithDescription("Возвращает запись дополнительной информации по идентификатору.");
    }
}
