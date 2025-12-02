namespace UserProfile.TimeCafe.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "TimeCafe UserProfile API", Version = "v1" });
            c.EnableAnnotations();
            c.ExampleFilters();
        });
        services.AddSwaggerExamples();
        services.AddSwaggerExamplesFromAssemblyOf<CreateProfileDtoExample>();

        return services;
    }

    public static void UseSwaggerDevelopment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeCafe UserProfile API v1");
                c.RoutePrefix = string.Empty;
            });
            app.UseScalarConfiguration();
        }
    }
}
