namespace Billing.TimeCafe.API.Filters;

public class RouteParameterExamplesFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var examples = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["userId"] = new GetBalanceDtoExample().GetExamples().UserId.ToString(),
            ["transactionId"] = new GetTransactionDtoExample().GetExamples().TransactionId.ToString()
        };

        foreach (var parameter in operation.Parameters)
        {
            if (parameter.In == ParameterLocation.Path && examples.TryGetValue(parameter.Name, out var value))
            {
                parameter.Example = new OpenApiString(value);
            }
        }
    }
}
