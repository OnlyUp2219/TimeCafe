namespace UserProfile.TimeCafe.API.Endpoints.ProfilePhoto;

public class GetPhotoProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/S3/image/{userId}", async (
            string userId,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetProfilePhotoQuery(userId);
            var result = await sender.Send(query, ct);
            return result.ToHttpResultV2(r => Results.File(r.Stream!, r.ContentType!, enableRangeProcessing: true));
        })
        .WithTags("ProfilePhoto")
        .WithName("GetProfilePhoto")
        .WithSummary("Получить фото профиля из S3")
        .WithDescription("Загружает фото пользователя из S3 хранилища и возвращает его как файловый стрим. Поддерживает Range-запросы.")
        .WithOpenApi(op =>
        {
            op.Parameters[0].Example = new Microsoft.OpenApi.Any.OpenApiString("30a3d946-97f8-470f-98b0-3c1230c09dc6");
            return op;
        })
        .RequireAuthorization();
    }
}
