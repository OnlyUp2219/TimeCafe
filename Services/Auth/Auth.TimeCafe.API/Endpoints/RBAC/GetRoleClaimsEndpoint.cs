using Auth.TimeCafe.Application.CQRS.RBAC.Query;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public class GetRoleClaimsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .RequireAuthorization()
            .MapGet("/role-claims", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetRoleClaimsQuery(), ct);

                return result.ToHttpResult(roleClaims => Results.Ok(new { roleClaims }));
            })
            .WithName("GetRoleClaims")
            .WithSummary("Получение ролей и их разрешений")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Возвращает список ролей и связанных с ними разрешений.");
    }
}
