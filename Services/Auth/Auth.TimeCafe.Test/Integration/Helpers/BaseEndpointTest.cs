using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Auth.TimeCafe.Test.Integration.Helpers;

public abstract class BaseEndpointTest : IClassFixture<IntegrationApiFactory>
{
    protected readonly HttpClient Client;
    protected readonly IntegrationApiFactory Factory;

    public BaseEndpointTest(IntegrationApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected void SeedUser(string email, string password, bool emailConfirmed)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = userManager.FindByEmailAsync(email).Result;
        if (user == null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = emailConfirmed };
            userManager.CreateAsync(user, password).Wait();
        }
        else if (user.EmailConfirmed != emailConfirmed)
        {
            user.EmailConfirmed = emailConfirmed;
            userManager.UpdateAsync(user).Wait();
        }
    }

    protected HttpClient CreateClientWithDisabledRateLimiter()
    {
        var overrides = new Dictionary<string, string?>
        {
            ["RateLimiter:EmailSms:MinIntervalSeconds"] = "0",
            ["RateLimiter:EmailSms:WindowMinutes"] = "1",
            ["RateLimiter:EmailSms:MaxRequests"] = "10000"
        };

        return Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, conf) => conf.AddInMemoryCollection(overrides));
        }).CreateClient();
    }

    protected async Task<string> GenerateConfirmationTokenAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByEmailAsync(email);
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user!);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    protected async Task<bool> IsEmailConfirmedAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByEmailAsync(email);
        return user!.EmailConfirmed;
    }

    protected async Task<(string userId, string accessToken)> CreateAuthenticatedUserAsync()
    {
        var email = $"user_{Guid.NewGuid():N}@example.com";
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        await userManager.CreateAsync(user, "P@ssw0rd!");

        var loginDto = new { Email = email, Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt", loginDto);
        loginResp.EnsureSuccessStatusCode();
        var json = JsonDocument.Parse(await loginResp.Content.ReadAsStringAsync()).RootElement;
        var token = json.GetProperty("accessToken").GetString()!;

        return (user.Id, token);
    }
}
