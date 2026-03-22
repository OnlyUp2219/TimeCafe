namespace Billing.TimeCafe.API.DTOs;

public record GetBalanceDto
{
    public string UserId { get; set; } = string.Empty;
}

public class GetBalanceExampleFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        _ = operation;
        _ = context;
    }
}
