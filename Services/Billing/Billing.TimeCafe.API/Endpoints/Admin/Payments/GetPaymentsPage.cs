namespace Billing.TimeCafe.API.Endpoints.Admin.Payments;

public class GetPaymentsPageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/payments", async (
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Guid? userId,
            [FromServices] ISender sender) =>
        {
            var query = new GetPaymentsPageQuery(page, pageSize, userId);
            var result = await sender.Send(query);
            
            return result.ToHttpResult(data => TypedResults.Ok(data));
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
