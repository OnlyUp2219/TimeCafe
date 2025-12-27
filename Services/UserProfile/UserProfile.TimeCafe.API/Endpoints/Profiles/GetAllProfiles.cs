namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class GetAllProfiles : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles", async (
            [FromServices] ISender sender) =>
        {
            var query = new GetAllProfilesQuery();
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(r.Profiles));
        })
        .WithTags("Profiles")
        .WithName("GetAllProfiles")
        .WithSummary("Получить все профили")
        .WithDescription("Возвращает список всех профилей пользователей.")
        .RequireAuthorization();
    }
}
