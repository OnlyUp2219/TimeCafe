namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class CreateEmptyProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/profiles/empty/{userId}", async (
            [FromServices] ISender sender,
            [AsParameters] CreateEmptyProfileDto dto) =>
        {
            var command = new CreateEmptyCommand((dto.UserId));
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Json(new { message = r.Message }, statusCode: r.StatusCode ?? 201));
        })
        .WithTags("Profiles")
        .WithName("CreateEmptyProfile")
        .WithSummary("Создать пустой профиль")
        .WithDescription("Создаёт пустой профиль пользователя с только UserId.")
        .RequireAuthorization();
    }
}
