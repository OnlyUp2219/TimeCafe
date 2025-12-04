namespace Venue.TimeCafe.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "TimeCafe Venue API", Version = "v1" });
            c.ExampleFilters();
        });
        services.AddSwaggerExamples();

        services.AddSwaggerExamplesFromAssemblyOf<CreateVisitDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<EndVisitDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<DeleteVisitDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<UpdateVisitDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<GetVisitByIdDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<GetActiveVisitByUserDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<HasActiveVisitDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<GetVisitHistoryDtoExample>();

        services.AddSwaggerExamplesFromAssemblyOf<CreateTariffDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<UpdateTariffDtoExample>();

        services.AddSwaggerExamplesFromAssemblyOf<CreatePromotionDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<UpdatePromotionDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<DeletePromotionDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<ActivatePromotionDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<DeactivatePromotionDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<GetPromotionByIdDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<GetActivePromotionsByDateDtoExample>();

        services.AddSwaggerExamplesFromAssemblyOf<CreateThemeDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<UpdateThemeDtoExample>();

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
