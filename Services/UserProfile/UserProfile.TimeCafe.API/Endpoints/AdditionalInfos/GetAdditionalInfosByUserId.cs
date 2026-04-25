namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public class GetAdditionalInfosByUserId : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/{userId:guid}/infos", async (
            [FromServices] ISender sender,
            Guid userId,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize) =>
        {
            var query = new GetAdditionalInfosByUserIdQuery(userId, pageNumber ?? 1, pageSize ?? 10);
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("AdditionalInfos")
        .WithName("GetAdditionalInfosByUserId")
        .WithSummary("Список доп. информации для профиля")
        .WithDescription("Возвращает все записи дополнительной информации для пользователя.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.UserProfileAdditionalInfoRead));
    }
}

