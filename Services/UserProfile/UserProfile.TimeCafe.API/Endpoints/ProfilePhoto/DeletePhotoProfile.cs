namespace UserProfile.TimeCafe.API.Endpoints.ProfilePhoto;

public class DeletePhotoProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/S3/image/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId,
            CancellationToken ct) =>
        {
            var cmd = new DeleteProfilePhotoCommand(userId);
            var result = await sender.Send(cmd, ct);
            return result.ToHttpResult(() => TypedResults.NoContent());
        })
        .WithTags("ProfilePhoto")
        .WithName("DeleteProfilePhoto")
        .WithSummary("Удалить фото профиля из S3")
        .WithDescription("Удаляет фото пользователя из S3 хранилища и очищает PhotoUrl в профиле.")
        .Produces(204)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.UserProfilePhotoDelete));
    }
}

