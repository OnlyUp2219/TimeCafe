using Billing.TimeCafe.Application.CQRS.Payments.Queries;

namespace Billing.TimeCafe.API.Endpoints.Admin.Payments;

public sealed record GetPaymentsPageRequest(int Page = 1, int PageSize = 20, Guid? UserId = null);

public class GetPaymentsPageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/billing/admin")
            .WithTags("Admin - Billing")
            .MapGet("/payments", async (
                [AsParameters] GetPaymentsPageRequest request,
                [FromServices] ISender sender) =>
            {
                var query = new GetPaymentsPageQuery(request.Page, request.PageSize, request.UserId);
                var result = await sender.Send(query);
                return result.ToHttpResult(data => Results.Ok(new
                {
                    payments = data.Payments,
                    pagination = new
                    {
                        currentPage = request.Page,
                        pageSize = request.PageSize,
                        totalCount = data.TotalCount,
                        totalPages = data.TotalPages
                    }
                }));
            })
            .WithName("GetPaymentsPage")
            .WithSummary("Получение страницы платежей (admin)")
            .WithDescription("Возвращает страницу платежей с опциональной фильтрацией по userId.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingAdminRead));
    }
}
