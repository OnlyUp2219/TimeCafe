namespace Auth.TimeCafe.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "TimeCafe Auth API", Version = "v1" });
            c.EnableAnnotations();
            c.ExampleFilters();
        });
        services.AddSwaggerExamples();
        services.AddSwaggerExamplesFromAssemblyOf<RegisterDtoExample>();



        return services;
    }
}
