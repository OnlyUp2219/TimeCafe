namespace UserProfile.TimeCafe.Infrastructure;

public static class DependencyInjection
{
    private static readonly Type ApplicationAssemblyMarker =
        typeof(Application.CQRS.Profiles.Commands.CreateProfileCommand);

    public static IServiceCollection AddUserProfilePersistence(this IServiceCollection services)
    {
        _ = ApplicationAssemblyMarker;

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepositories, UserRepositories>();
        services.AddScoped<IAdditionalInfoRepository, AdditionalInfoRepository>();

        return services;
    }
}
