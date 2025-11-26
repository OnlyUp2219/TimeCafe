using UserProfile.TimeCafe.Domain.Models;
using UserProfile.TimeCafe.Infrastructure.Data;

namespace UserProfile.TimeCafe.Test.Integration.Helpers;

public abstract class BaseEndpointTest : IClassFixture<IntegrationApiFactory>
{
    protected readonly HttpClient Client;
    protected readonly IntegrationApiFactory Factory;

    public BaseEndpointTest(IntegrationApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task SeedProfileAsync(string userId, string firstName, string lastName, Gender gender = Gender.NotSpecified)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var existing = await context.Profiles.FindAsync(userId);
        if (existing == null)
        {
            var profile = new Profile
            {
                UserId = userId,
                FirstName = firstName,
                LastName = lastName,
                Gender = gender,
                ProfileStatus = ProfileStatus.Completed,
                CreatedAt = DateTime.UtcNow
            };
            context.Profiles.Add(profile);
            await context.SaveChangesAsync();
        }
    }
}
