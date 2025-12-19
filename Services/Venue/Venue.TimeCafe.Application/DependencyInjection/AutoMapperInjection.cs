namespace Venue.TimeCafe.Application.DependencyInjection;

public static class AutoMapperInjection
{
    public static IServiceCollection AddVenueAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(VisitProfile));

        return services;
    }
}