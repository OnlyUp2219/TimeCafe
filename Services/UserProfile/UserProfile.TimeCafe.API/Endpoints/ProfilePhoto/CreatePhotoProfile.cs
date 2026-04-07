namespace UserProfile.TimeCafe.API.Endpoints.ProfilePhoto;

public class CreatePhotoProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/S3/image/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId,
            IFormFile file,
            CancellationToken ct) =>
        {
            var stream = file.OpenReadStream();
            var cmd = new UploadProfilePhotoCommand(userId, stream, file.ContentType, file.FileName, file.Length);
            var result = await sender.Send(cmd, ct);
            return result.ToHttpResult(r =>
                Results.Created($"/S3/image/{userId}", new { r.Key, r.Url, r.Size, r.ContentType }));
        })
        .Accepts<IFormFile>("multipart/form-data")
        .DisableAntiforgery()
        .WithTags("ProfilePhoto")
        .WithName("UploadProfilePhoto")
        .WithSummary("Загрузить фото профиля в S3")
        .WithDescription("Загружает фото пользователя в S3 хранилище и обновляет PhotoUrl в профиле. Принимает multipart/form-data с файлом изображения.")
        .Produces(201)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.UserProfilePhotoCreate));
    }
}
