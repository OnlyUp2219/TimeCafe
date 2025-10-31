namespace Auth.TimeCafe.API.Extensions;

public static class DatabaseExtension
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register repositories
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
}
