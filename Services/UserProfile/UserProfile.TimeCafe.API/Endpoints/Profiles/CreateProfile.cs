namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class CreateProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/profiles", async (
            ISender sender,
            [FromBody] CreateProfileDto dto) =>
        {
            var command = new CreateProfileCommand(dto.UserId, dto.FirstName, dto.LastName, dto.Gender);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Json(new { message = r.Message, profile = r.Profile }, statusCode: r.StatusCode ?? 201));
        })
        .WithTags("Profiles")
        .WithName("CreateProfile")
        .WithSummary("Создать профиль")
        .WithDescription("Создаёт новый профиль пользователя.")
        .RequireAuthorization();
    }
}
