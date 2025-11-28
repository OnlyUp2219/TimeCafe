namespace UserProfile.TimeCafe.API.Endpoints.ProfilePhoto;

public class TestPhotoProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/S3/test/user/{userId}/photo", async (
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
        })
        .WithTags("ProfilePhoto")
        .WithName("TestUserPhoto")
        .WithSummary("Тестовый эндпоинт: создать пользователя и получить фото")
        .WithDescription("Проверяет существование пользователя. Если нет — создаёт тестового пользователя. Если есть фото в S3 — возвращает его, иначе возвращает JSON с информацией о статусе.");
    }
}
