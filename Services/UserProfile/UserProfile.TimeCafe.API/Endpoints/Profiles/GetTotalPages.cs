namespace UserProfile.TimeCafe.API.Endpoints.Profiles;

public class GetTotalPages : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/total", async
        ([FromServices] ISender sender) =>
        {
            var query = new GetTotalPagesQuery();
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { totalCount = r.TotalCount }));
        })
        .WithTags("Profiles")
        .WithName("GetTotalPages")
        .WithSummary("Получить общее количество профилей")
        .WithDescription("Возвращает общее количество профилей в системе.")
        .Produces(200)
        .RequireAuthorization();
    }
}
