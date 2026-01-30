namespace UserProfile.TimeCafe.Infrastructure;

public static class DependencyInjection
{
    private static readonly Type ApplicationAssemblyMarker =
        typeof(UserProfile.TimeCafe.Application.CQRS.Profiles.Commands.CreateProfileCommand);

    public static IServiceCollection AddUserProfilePersistence(this IServiceCollection services)
    {
        _ = ApplicationAssemblyMarker;

        services.AddScoped<IUserRepositories, UserRepositories>();
        services.AddScoped<IAdditionalInfoRepository, AdditionalInfoRepository>();

        return services;
    }
}
