using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.TimeCafe.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var admin = configuration.GetSection("Seed:Admin");

        var adminEmail = admin["Email"] ?? "";
        var adminPassword = admin["Password"] ?? "";
        var adminRole = admin["Role"] ?? "";

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            throw new InvalidOperationException("Seed:Admin credentials missing in configuration.");


        if (!await roleManager.RoleExistsAsync(adminRole))
            await roleManager.CreateAsync(new IdentityRole(adminRole));

        var user = await userManager.FindByEmailAsync(adminEmail);
        if (user == null)
        {
            user = new IdentityUser
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
}