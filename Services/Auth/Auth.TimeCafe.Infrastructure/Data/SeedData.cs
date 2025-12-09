namespace Auth.TimeCafe.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var admin = configuration.GetSection("Seed:Admin");

        var adminEmail = admin["Email"] ?? "";
        var adminPassword = admin["Password"] ?? "";
        var adminRole = admin["Role"] ?? "";

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            throw new InvalidOperationException("Seed:Admin credentials missing in configuration.");


        if (!await roleManager.RoleExistsAsync(adminRole))
            await roleManager.CreateAsync(new IdentityRole<Guid>(adminRole));

        var user = await userManager.FindByEmailAsync(adminEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, adminRole);
        }
    }

    public static async Task SeedLoadTestUserAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var load = configuration.GetSection("Seed:LoadUser");
        var email = load["Email"] ?? "load.user@example.com";
        var password = load["Password"] ?? "P@ssw0rd!";

        var existing = await userManager.FindByEmailAsync(email);
        if (existing != null)
            return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(user, password);
    }
}