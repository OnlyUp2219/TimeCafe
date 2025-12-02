namespace UserProfile.TimeCafe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserProfilePersistence(this IServiceCollection services)
    {
        services.AddScoped<IUserRepositories, UserRepositories>();
        services.AddScoped<IAdditionalInfoRepository, AdditionalInfoRepository>();

        return services;
    }
}
