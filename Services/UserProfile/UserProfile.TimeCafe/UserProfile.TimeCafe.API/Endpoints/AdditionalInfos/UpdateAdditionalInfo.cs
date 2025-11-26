namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public class UpdateAdditionalInfo : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/infos", async (
            ISender sender,
            [FromBody] UpdateAdditionalInfoDto dto) =>
        {
            var info = new AdditionalInfo
            {
                InfoId = dto.InfoId,
                UserId = dto.UserId,
                InfoText = dto.InfoText,
                CreatedBy = dto.CreatedBy
            };
            var command = new UpdateAdditionalInfoCommand(info);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message, info = r.AdditionalInfo }));
        })
        .WithTags("AdditionalInfos")
        .WithName("UpdateAdditionalInfo")
        .WithSummary("Обновить запись доп. информации")
        .WithDescription("Обновляет существующую запись дополнительной информации.");
    }
}
