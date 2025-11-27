namespace UserProfile.TimeCafe.API.Endpoints.ProfilePhoto;

/// <summary>
/// Эндпоинты для работы с фото профиля (S3).
/// 
/// ANTIFORGERY POLICY:
/// - DisableAntiforgery() используется для чистого API (JWT/Bearer токены, мобильные клиенты).
/// - Если загрузка идёт из браузерных форм с cookie-аутентификацией, замените на .RequireAntiforgery()
///   и добавьте app.UseAntiforgery() в Program.cs после UseAuthentication/UseAuthorization.
/// - Для переключения можно добавить конфиг PhotoApi:RequireAntiforgery и читать из IConfiguration.
/// </summary>
public class CreatePhotoProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("S3").WithTags("S3Photo").DisableAntiforgery();

        group.MapPost("image/{userId}", async (
            string userId,
            ISender sender,
            IFormFile file,
            CancellationToken ct) =>
        {
            if (file is null)
                return Results.BadRequest("Файл обязателен");
            var stream = file.OpenReadStream();
            var cmd = new UploadProfilePhotoCommand(userId, stream, file.ContentType, file.FileName, file.Length);
            var result = await sender.Send(cmd, ct);
            return result.ToHttpResultV2(r =>
                Results.Created($"/S3/image/{userId}", new { r.Key, r.Url, r.Size, r.ContentType }));
        })
        .Accepts<IFormFile>("multipart/form-data");

        group.MapGet("image/{userId}", async (
            string userId,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetProfilePhotoQuery(userId);
            var result = await sender.Send(query, ct);
            return result.ToHttpResultV2(r => Results.File(r.Stream!, r.ContentType!, enableRangeProcessing: true));
        });

        group.MapDelete("image/{userId}", async (
            string userId,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new DeleteProfilePhotoCommand(userId);
            var result = await sender.Send(cmd, ct);
            return result.ToHttpResultV2(_ => Results.NoContent());
        });

        // Тестовый эндпоинт для проверки наличия пользователя и получения фото
        group.MapGet("test/user/{userId}/photo", async (
            string userId,
            ISender sender,
            IUserRepositories userRepo,
            IProfilePhotoStorage photoStorage,
            CancellationToken ct) =>
        {
            var profile = await userRepo.GetProfileByIdAsync(userId, ct);

            if (profile is null)
            {
                profile = new Profile
                {
                    UserId = userId,
                    FirstName = "Test",
                    LastName = "User",
                    Gender = Gender.NotSpecified,
                    ProfileStatus = ProfileStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                };
                await userRepo.CreateProfileAsync(profile, ct);

                return Results.Json(new
                {
                    message = "Пользователь создан, но фото отсутствует",
                    userId = profile.UserId,
                    photoUrl = profile.PhotoUrl,
                    hasPhoto = false
                });
            }
            if (string.IsNullOrEmpty(profile.PhotoUrl))
            {
                return Results.Json(new
                {
                    message = "Пользователь существует, но фото отсутствует",
                    userId = profile.UserId,
                    photoUrl = profile.PhotoUrl,
                    hasPhoto = false
                });
            }

            var photoDto = await photoStorage.GetAsync(userId, ct);

            if (photoDto is null)
            {
                return Results.Json(new
                {
                    message = "PhotoUrl указан, но файл в S3 не найден",
                    userId = profile.UserId,
                    photoUrl = profile.PhotoUrl,
                    hasPhoto = false
                });
            }

            return Results.File(photoDto.Stream, photoDto.ContentType, enableRangeProcessing: true);
        });
    }
}
