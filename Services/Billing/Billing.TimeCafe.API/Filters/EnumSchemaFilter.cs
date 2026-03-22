namespace Billing.TimeCafe.API.Filters;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        _ = schema;
        _ = context;
    }
}
