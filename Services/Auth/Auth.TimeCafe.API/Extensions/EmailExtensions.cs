namespace Auth.TimeCafe.API.Extensions;

public static class EmailExtensions
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<PostmarkOptions>(configuration, "Postmark");

        services.AddScoped<PostmarkEmailSender>();
        services.AddScoped<IEmailSender<ApplicationUser>>(sp =>
            new BackgroundEmailSender(sp.GetRequiredService<PostmarkEmailSender>(), sp.GetRequiredService<ILogger<BackgroundEmailSender>>()));
        services.AddHttpClient();

        return services;
    }
}
