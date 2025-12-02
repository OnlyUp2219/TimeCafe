namespace Venue.TimeCafe.API.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddVenueDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }
}
