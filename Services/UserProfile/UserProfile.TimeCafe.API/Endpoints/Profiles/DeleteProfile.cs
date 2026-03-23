namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class DeleteProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/profiles/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId) =>
        {
            var command = new DeleteProfileCommand(userId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Profiles")
        .WithName("DeleteProfile")
        .WithSummary("Удалить профиль")
        .WithDescription("Удаляет профиль пользователя по UserId.")
        .RequireAuthorization();
    }
}
