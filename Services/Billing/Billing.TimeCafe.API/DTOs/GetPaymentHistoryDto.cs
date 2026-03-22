namespace Billing.TimeCafe.API.DTOs;

public record GetPaymentHistoryDto(
    [FromRoute] string UserId,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 20);

public class GetPaymentHistoryDtoExampleFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        _ = operation;
        _ = context;
    }
}
