using Venue.TimeCafe.API.DTOs.Promotion;
using Venue.TimeCafe.API.DTOs.Tariff;
using Venue.TimeCafe.API.DTOs.Theme;
using Venue.TimeCafe.API.DTOs.Visit;

namespace Venue.TimeCafe.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "TimeCafe Venue API", Version = "v1" });
            c.EnableAnnotations();
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
}
