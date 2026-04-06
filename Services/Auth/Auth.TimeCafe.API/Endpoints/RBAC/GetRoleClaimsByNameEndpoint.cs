using Auth.TimeCafe.Application.CQRS.RBAC.Query;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public class GetRoleClaimsByNameEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .RequireAuthorization()
            .MapGet("/role-claims/{roleName}", async (
                [FromServices] ISender sender,
                [FromRoute] string roleName,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetRoleClaimsByNameQuery(roleName), ct);

                return result.ToHttpResult(roleClaim => Results.Ok(new { roleClaim }));
            })
            .WithName("GetRoleClaimsByName")
            .WithSummary("Получение разрешений роли по имени")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Возвращает список разрешений для указанной роли.");
    }
}
