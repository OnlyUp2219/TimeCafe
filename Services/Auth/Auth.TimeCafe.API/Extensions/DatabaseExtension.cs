namespace Auth.TimeCafe.API.Extensions;

public static class DatabaseExtension
{
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
