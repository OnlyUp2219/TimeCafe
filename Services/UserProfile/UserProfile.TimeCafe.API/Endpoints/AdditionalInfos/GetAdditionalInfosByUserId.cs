namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public class GetAdditionalInfosByUserId : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/{userId}/infos", async (
            [FromServices] ISender sender,
            [AsParameters] GetAdditionalInfosByUserIdDto dto) =>
        {
            var query = new GetAdditionalInfosByUserIdQuery(dto.UserId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(r.AdditionalInfos));
        })
        .WithTags("AdditionalInfos")
        .WithName("GetAdditionalInfosByUserId")
        .WithSummary("Список доп. информации для профиля")
        .WithDescription("Возвращает все записи дополнительной информации для пользователя.")
        .RequireAuthorization();
    }
}
