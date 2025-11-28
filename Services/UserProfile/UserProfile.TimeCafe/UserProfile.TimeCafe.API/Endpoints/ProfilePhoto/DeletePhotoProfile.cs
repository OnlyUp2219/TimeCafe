namespace UserProfile.TimeCafe.API.Endpoints.ProfilePhoto;

public class DeletePhotoProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/S3/image/{userId}", async (
            string userId,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new DeleteProfilePhotoCommand(userId);
            var result = await sender.Send(cmd, ct);
            return result.ToHttpResultV2(_ => Results.NoContent());
        })
        .WithTags("ProfilePhoto")
        .WithName("DeleteProfilePhoto")
        .WithSummary("Удалить фото профиля из S3")
        .WithDescription("Удаляет фото пользователя из S3 хранилища и очищает PhotoUrl в профиле.");
    }
}
