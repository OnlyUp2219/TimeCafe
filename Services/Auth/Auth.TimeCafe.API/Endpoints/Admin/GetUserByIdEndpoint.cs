using Auth.TimeCafe.Application.CQRS.Admin.Query;

namespace Auth.TimeCafe.API.Endpoints.Admin;

public class GetUserByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/admin")
            .WithTags("Admin")
            .MapGet("/users/{userId:guid}", async (
                Guid userId,
                [FromServices] ISender sender) =>
            {
                var query = new GetUserByIdQuery(userId);
                var result = await sender.Send(query);

                return result.ToHttpResult(user => Results.Ok(new { user }));
            })
            .WithName("GetUserById")
            .WithSummary("Получение пользователя по ID")
            .WithDescription("Возвращает детальную информацию о пользователе по его ID.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.AccountAdminRead));
    }
}
