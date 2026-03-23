namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public record CreateAdditionalInfoRequest(Guid UserId, string InfoText, string? CreatedBy);

public class CreateAdditionalInfo : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/infos", async (
            [FromServices] ISender sender,
            [FromBody] CreateAdditionalInfoRequest request) =>
        {
            var command = new CreateAdditionalInfoCommand(request.UserId, request.InfoText, request.CreatedBy);
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
