namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public class DeleteAdditionalInfo : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/infos/{infoId}", async (
            [FromServices] ISender sender,
            [AsParameters] DeleteAdditionalInfoDto dto) =>
        {
            var command = new DeleteAdditionalInfoCommand(dto.InfoId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("AdditionalInfos")
        .WithName("DeleteAdditionalInfo")
        .WithSummary("Удалить запись доп. информации")
        .WithDescription("Удаляет запись дополнительной информации по идентификатору.")
        .RequireAuthorization();
    }
}
