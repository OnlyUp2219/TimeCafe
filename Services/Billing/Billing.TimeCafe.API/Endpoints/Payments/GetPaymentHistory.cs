namespace Billing.TimeCafe.API.Endpoints.Payments;

using Billing.TimeCafe.Application.CQRS.Payments.Queries;

public class GetPaymentHistory : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/billing/payments/history/{userId}", async (
            [FromServices] ISender sender,
            [AsParameters] GetPaymentHistoryDto dto) =>
        {
            var query = new GetPaymentHistoryQuery(dto.UserId, dto.Page, dto.PageSize);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new
            {
                payments = r.Payments,
                pagination = new
                {
                    currentPage = dto.Page,
                    pageSize = dto.PageSize,
                    totalCount = r.TotalCount,
                    totalPages = r.TotalPages
                }
            }));
        })
        .WithTags("Payments")
        .WithName("GetPaymentHistory")
        .WithSummary("История платежей пользователя")
        .WithDescription("Возвращает историю платежей с пагинацией (по 20 по умолчанию, макс 100).")
        .WithOpenApi()
        .RequireAuthorization();
    }
}
