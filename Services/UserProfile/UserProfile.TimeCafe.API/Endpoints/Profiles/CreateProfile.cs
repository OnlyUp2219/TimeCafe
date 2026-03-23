namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public record CreateProfileRequest(Guid UserId, string FirstName, string LastName, Gender Gender);

public class CreateProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/profiles", async (
            [FromServices] ISender sender,
            [FromBody] CreateProfileRequest request) =>
        {
            var command = new CreateProfileCommand(request.UserId, request.FirstName, request.LastName, request.Gender);
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
