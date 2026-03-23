namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public record UpdateAdditionalInfoRequest(Guid UserId, string InfoText, string? CreatedBy);

public class UpdateAdditionalInfo : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/infos/{infoId:guid}", async (
            ISender sender,
            Guid infoId,
            [FromBody] UpdateAdditionalInfoRequest request) =>
        {
            var command = new UpdateAdditionalInfoCommand(infoId, request.UserId, request.InfoText, request.CreatedBy);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message, info = r.AdditionalInfo }));
        })
        .WithTags("AdditionalInfos")
        .WithName("UpdateAdditionalInfo")
        .WithSummary("Обновить запись доп. информации")
        .WithDescription("Обновляет существующую запись дополнительной информации.")
        .RequireAuthorization();
    }
}
