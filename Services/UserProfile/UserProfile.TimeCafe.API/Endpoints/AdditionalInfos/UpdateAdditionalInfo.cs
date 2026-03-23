namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public record UpdateAdditionalInfoRequest(
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    Guid UserId,
    /// <example>Предпочитает место у окна</example>
    string InfoText,
    /// <example>admin</example>
    string? CreatedBy);

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
        .Produces(200)
        .Produces(404)
        .RequireAuthorization();
    }
}
