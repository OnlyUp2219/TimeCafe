namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public record CreateProfileRequest(
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    Guid UserId,
    /// <example>Иван</example>
    string FirstName,
    /// <example>Иванов</example>
    string LastName,
    /// <example>Male</example>
    Gender Gender);

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
            return result.ToHttpResult(r => TypedResults.Created($"/profiles/{r.UserId}", r));
        })
        .WithTags("Profiles")
        .WithName("CreateProfile")
        .WithSummary("Создать профиль")
        .WithDescription("Создаёт новый профиль пользователя.")
        .Produces(201)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.UserProfileProfileCreate));
    }
}

