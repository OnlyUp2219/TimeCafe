namespace Auth.TimeCafe.API.Extensions;

public static class EmailExtensions
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PostmarkOptions>(configuration.GetSection("Postmark"));
        services.AddSingleton<PostmarkEmailSender>();
        services.AddSingleton<IEmailSender<IdentityUser>>(sp =>
            new BackgroundEmailSender(sp.GetRequiredService<PostmarkEmailSender>(), sp.GetRequiredService<ILogger<BackgroundEmailSender>>()));
        services.AddHttpClient();

        return services;
    }
}
