namespace Billing.TimeCafe.API.Endpoints.Admin.Payments;

public class GetPaymentsPageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/payments", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] Guid? userId,
            [FromServices] ISender sender) =>
        {
            var query = new GetPaymentsPageQuery(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, userId);
            var result = await sender.Send(query);
            return result.ToHttpResult(data => Results.Ok(new
            {
                payments = data.Payments,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount = data.TotalCount,
                    totalPages = data.TotalPages
                }
            }));
        })
        .WithTags("Admin - Billing")
        .WithName("GetPaymentsPage")
        .WithSummary("Получение страницы платежей (admin)")
        .WithDescription("Возвращает страницу платежей с опциональной фильтрацией по userId.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingAdminRead));
    }
}
