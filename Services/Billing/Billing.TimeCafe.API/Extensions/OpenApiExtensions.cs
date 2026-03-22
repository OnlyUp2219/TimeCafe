namespace Billing.TimeCafe.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
            };

            c.SwaggerDoc("v1", new() { Title = "TimeCafe Billing API", Version = "v1" });
            c.ExampleFilters();
            c.OperationFilter<RouteParameterExamplesFilter>();
            c.SchemaFilter<EnumSchemaFilter>();

            c.AddSecurityDefinition("Bearer", bearerScheme);

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
