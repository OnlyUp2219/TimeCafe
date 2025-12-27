namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class GetProfileById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/{userId}", async (
            [FromServices] ISender sender,
            [AsParameters] GetProfileByIdDto dto) =>
        {
            var query = new GetProfileByIdQuery(dto.UserId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(r.Profile));
        })
        .WithTags("Profiles")
        .WithName("GetProfileById")
        .WithSummary("Получить профиль по UserId")
        .WithDescription("Возвращает профиль пользователя по идентификатору UserId.")
        .RequireAuthorization();
    }
}
