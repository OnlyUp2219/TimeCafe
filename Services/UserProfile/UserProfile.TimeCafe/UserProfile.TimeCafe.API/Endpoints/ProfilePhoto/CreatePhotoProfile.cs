namespace UserProfile.TimeCafe.API.Endpoints.ProfilePhoto;

public class CreatePhotoProfile : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("S3").WithTags("S3Photo");

        group.MapPost("image/{userId}", async (
            string userId,
            ISender sender,
            [FromForm(Name = "file")] IFormFile file,
            CancellationToken ct) =>
        {
            if (file is null)
                return Results.BadRequest("Файл обязателен");
            var stream = file.OpenReadStream();
            var cmd = new UploadProfilePhotoCommand(userId, stream, file.ContentType, file.FileName, file.Length);
            var result = await sender.Send(cmd, ct);
            return result.ToHttpResultV2(r =>
                Results.Created($"/S3/image/{userId}", new { r.Key, r.Url, r.Size, r.ContentType }));
        });

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
    }
}
