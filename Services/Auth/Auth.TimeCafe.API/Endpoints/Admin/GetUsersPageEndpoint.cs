using Auth.TimeCafe.Application.CQRS.Admin.Query;

namespace Auth.TimeCafe.API.Endpoints.Admin;

public sealed record GetUsersPageRequest(int Page, int Size, string? Search, string? Status);

public class GetUsersPageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/admin")
            .WithTags("Admin")
            .MapGet("/users", async (
                [AsParameters] GetUsersPageRequest request,
                [FromServices] ISender sender) =>
            {
                var query = new GetUsersPageQuery(request.Page, request.Size, request.Search, request.Status);
                var result = await sender.Send(query);

                return result.ToHttpResult(data => Results.Ok(new
                {
                    users = data.Users,
                    pagination = new
                    {
                        currentPage = request.Page,
                        pageSize = request.Size,
                        totalCount = data.TotalCount,
                        totalPages = (int)Math.Ceiling(data.TotalCount / (double)request.Size)
                    }
                }));
            })
            .WithName("GetUsersPage")
            .WithSummary("Получение страницы пользователей для админки")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.AccountAdminRead))
            .WithDescription("Возвращает страницу пользователей с фильтрами и пагинацией.");
    }
}
