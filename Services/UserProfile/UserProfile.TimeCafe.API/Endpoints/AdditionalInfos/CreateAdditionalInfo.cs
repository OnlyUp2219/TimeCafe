namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public record CreateAdditionalInfoRequest(
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    Guid UserId,
    /// <example>Предпочитает место у окна</example>
    string InfoText,
    /// <example>admin</example>
    string? CreatedBy);

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
            return result.ToHttpResult(r => TypedResults.Created($"/infos/{r.InfoId}", r));
        })
        .WithTags("AdditionalInfos")
        .WithName("CreateAdditionalInfo")
        .WithSummary("Создать запись доп. информации")
        .WithDescription("Создаёт новую запись дополнительной информации для пользователя.")
        .Produces(201)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.UserProfileAdditionalInfoCreate));
    }
}

