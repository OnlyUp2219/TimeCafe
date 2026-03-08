namespace Venue.TimeCafe.Application.DependencyInjection;

public static class AutoMapperInjection
{
    public static IServiceCollection AddVenueAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(VisitProfile));

        return services;
    }
}