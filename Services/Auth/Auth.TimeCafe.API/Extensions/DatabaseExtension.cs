namespace Auth.TimeCafe.API.Extensions;

public static class DatabaseExtension
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        if (app.Configuration.GetValue<bool>("SkipMigrations"))
            return;

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync();
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        var roleService = scope.ServiceProvider.GetRequiredService<IUserRoleService>();
        await roleService.EnsureRolesCreatedAsync();
        await SeedData.SeedAdminAsync(scope.ServiceProvider);
        await SeedData.SeedLoadTestUserAsync(scope.ServiceProvider);
    }
}
