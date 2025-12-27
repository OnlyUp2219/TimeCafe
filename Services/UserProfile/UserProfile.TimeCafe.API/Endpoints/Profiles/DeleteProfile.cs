namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class DeleteProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/profiles/{userId}", async (
            [FromServices] ISender sender,
            [AsParameters] DeleteProfileDto dto) =>
        {
            var command = new DeleteProfileCommand(dto.UserId);
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
