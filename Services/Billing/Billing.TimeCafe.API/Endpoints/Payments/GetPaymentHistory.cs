namespace Billing.TimeCafe.API.Endpoints.Payments;

public class GetPaymentHistory : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/payments/history/{userId:guid}", async (
            [FromServices] ISender sender,
            [FromRoute] Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var query = new GetPaymentHistoryQuery(userId, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize);
            var result = await sender.Send(query);
            
            return result.ToHttpResult(onSuccess: r => TypedResults.Ok(new
            {
                payments = r.Payments,
                pagination = new
                {
                    currentPage = page <= 0 ? 1 : page,
                    pageSize = pageSize <= 0 ? 20 : pageSize,
                    totalCount = r.TotalCount,
                    totalPages = r.TotalPages
                }
            }));
        })
        .WithTags("Payments")
        .WithName("GetPaymentHistory")
        .WithSummary("История платежей пользователя")
        .WithDescription("Возвращает историю платежей с пагинацией (по 20 по умолчанию, макс 100).")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingPaymentHistoryRead));
    }
}
