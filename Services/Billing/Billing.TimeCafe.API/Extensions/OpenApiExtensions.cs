namespace Billing.TimeCafe.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "TimeCafe Billing API", Version = "v1" });
            c.ExampleFilters();
            c.OperationFilter<RouteParameterExamplesFilter>();
            c.SchemaFilter<EnumSchemaFilter>();
        });
        services.AddSwaggerExamples();

        services.AddSwaggerExamplesFromAssemblyOf<AdjustBalanceDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<GetTransactionDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<GetTransactionHistoryDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<GetUserDebtDtoExample>();

        return services;
    }

    public static WebApplication UseSwaggerDevelopment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseScalarConfiguration();
        }

        return app;
    }
}
