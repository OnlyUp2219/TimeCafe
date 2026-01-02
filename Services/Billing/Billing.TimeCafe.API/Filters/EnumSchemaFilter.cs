namespace Billing.TimeCafe.API.Filters;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
            return;

        schema.Type = "integer";
        schema.Format = "int32";

        var enumNames = Enum.GetNames(context.Type);
        schema.Enum = enumNames
            .Select(name => new OpenApiInteger((int)Enum.Parse(context.Type, name)))
            .Cast<IOpenApiAny>()
            .ToList();

        var descriptions = enumNames
            .Select(name => $"{name}={(int)Enum.Parse(context.Type, name)}");
        schema.Description = string.Join(", ", descriptions);
    }
}
