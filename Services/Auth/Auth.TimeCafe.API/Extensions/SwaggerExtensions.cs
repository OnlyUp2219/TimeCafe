namespace Auth.TimeCafe.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
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

            c.SwaggerDoc("v1", new() { Title = "TimeCafe Auth API", Version = "v1" });
            c.EnableAnnotations();
            c.ExampleFilters();

            c.AddSecurityDefinition("Bearer", bearerScheme);

        });
        services.AddSwaggerExamples();
        services.AddSwaggerExamplesFromAssemblyOf<RegisterDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<LoginDtoExample>();
        services.AddSwaggerExamplesFromAssemblyOf<ChangePasswordRequestExample>();
        services.AddSwaggerExamplesFromAssemblyOf<ConfirmEmailRequestExample>();
        services.AddSwaggerExamplesFromAssemblyOf<ConfirmChangeEmailRequestExample>();
        services.AddSwaggerExamplesFromAssemblyOf<ChangeEmailRequestExample>();
        services.AddSwaggerExamplesFromAssemblyOf<ResendConfirmationRequestExample>();
        services.AddSwaggerExamplesFromAssemblyOf<ResetPasswordEmailRequestExample>();

        return services;
    }

    public static WebApplication UseSwaggerDevelopment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeCafe Auth API v1");
                c.RoutePrefix = string.Empty;
            });
            app.UseScalarConfiguration();
        }

        return app;
    }
}
