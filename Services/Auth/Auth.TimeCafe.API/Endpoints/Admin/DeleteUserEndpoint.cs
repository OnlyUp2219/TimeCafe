using Auth.TimeCafe.Application.CQRS.Admin.Command;

namespace Auth.TimeCafe.API.Endpoints.Admin;

public class DeleteUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/admin")
            .WithTags("Admin")
            .MapDelete("/users/{userId:guid}", async (
                Guid userId,
                [FromServices] ISender sender) =>
            {
                var command = new DeleteUserCommand(userId);
                var result = await sender.Send(command);
                return result.ToHttpResult(() => Results.Ok(new { message = "Пользователь удалён" }));
            })
            .WithName("DeleteUser")
            .WithSummary("Удаление пользователя (admin)")
            .WithDescription("Удаляет пользователя по ID. Операция необратима. Нельзя удалить собственный аккаунт.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.AccountAdminRead));
    }
}
