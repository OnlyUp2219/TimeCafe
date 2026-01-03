namespace Billing.TimeCafe.API.DTOs;

public record GetBalanceDto
{
    public Guid UserId { get; set; }
}

public class GetBalanceExampleFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var param = operation.Parameters.FirstOrDefault(p => p.Name == "userId");
        if (param != null)
        {
            param.Example = new OpenApiString("f47ac10b-58cc-4372-a567-0e02b2c3d479");
        }
    }
}
