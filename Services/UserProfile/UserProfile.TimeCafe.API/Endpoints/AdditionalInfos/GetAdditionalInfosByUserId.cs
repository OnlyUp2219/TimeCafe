namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public class GetAdditionalInfosByUserId : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/{userId:guid}/infos", async (
            [FromServices] ISender sender,
            Guid userId) =>
        {
            var query = new GetAdditionalInfosByUserIdQuery(userId);
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(r.AdditionalInfos));
        })
        .WithTags("AdditionalInfos")
        .WithName("GetAdditionalInfosByUserId")
        .WithSummary("Список доп. информации для профиля")
        .WithDescription("Возвращает все записи дополнительной информации для пользователя.")
        .Produces(200)
        .RequireAuthorization();
    }
}
