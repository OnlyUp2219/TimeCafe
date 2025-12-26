namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public class CreateAdditionalInfo : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/infos", async (
            ISender sender,
            [FromBody] CreateAdditionalInfoDto dto) =>
        {
            var command = new CreateAdditionalInfoCommand(dto.UserId, dto.InfoText, dto.CreatedBy);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Json(new { message = r.Message, info = r.AdditionalInfo }, statusCode: r.StatusCode ?? 201));
        })
        .WithTags("AdditionalInfos")
        .WithName("CreateAdditionalInfo")
        .WithSummary("Создать запись доп. информации")
        .WithDescription("Создаёт новую запись дополнительной информации для пользователя.")
        .RequireAuthorization();
    }
}
