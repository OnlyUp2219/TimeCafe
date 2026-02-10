namespace Billing.TimeCafe.API.DTOs;

public record GetPaymentHistoryDto(
    [FromRoute] string UserId,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 20);

public class GetPaymentHistoryDtoExampleFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var param = operation.Parameters.FirstOrDefault(p => p.Name == "userId");
        if (param != null)
        {
            param.Example = new OpenApiString("f47ac10b-58cc-4372-a567-0e02b2c3d479");

        }
        param = operation.Parameters.FirstOrDefault(p => p.Name == "page");
        if (param != null)
        {
            param.Example = new OpenApiInteger(1);
        }
        param = operation.Parameters.FirstOrDefault(p => p.Name == "pageSize");
        if (param != null)
        {
            param.Example = new OpenApiInteger(20);
        }
    }
}