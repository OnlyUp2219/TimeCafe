using Auth.TimeCafe.Application.CQRS.RBAC.Query;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public class RoleExistsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .RequireAuthorization()
            .MapGet("/roles/{roleName}/exists", async (
                [FromServices] ISender sender,
                [FromRoute] string roleName,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new RoleExistsQuery(roleName), ct);

                return result.ToHttpResult(exists => Results.Ok(new { roleName, exists }));
            })
            .WithName("RoleExists")
            .WithSummary("Проверка существования роли")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Возвращает признак существования роли по ее имени.");
    }
}
