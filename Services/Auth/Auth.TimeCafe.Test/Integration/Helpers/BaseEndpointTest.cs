
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
}
