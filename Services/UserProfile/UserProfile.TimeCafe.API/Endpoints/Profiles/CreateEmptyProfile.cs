namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class CreateEmptyProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/profiles/empty/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId) =>
        {
            var command = new CreateEmptyCommand(userId);
            var result = await sender.Send(command);
            return result.ToHttpResult(() => TypedResults.Created($"/profiles/{userId}", new { message = "Профиль создан" }));
        })
        .WithTags("Profiles")
        .WithName("CreateEmptyProfile")
        .WithSummary("Создать пустой профиль")
        .WithDescription("Создаёт пустой профиль пользователя с только UserId.")
        .Produces(201)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.UserProfileProfileCreate));
    }
}
