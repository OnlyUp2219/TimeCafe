namespace Venue.TimeCafe.Test.Integration.Repository;

public class LoyaltyCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnUserDiscountUpdated()
    {
        await ClearDatabaseAndCacheAsync();
        
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.UserLoyalties.RemoveRange(context.UserLoyalties);
            await context.SaveChangesAsync();
        }

        var userId = Guid.NewGuid();

        var initialResponse = await Client.GetAsync($"/venue/loyalty/{userId}");
        initialResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var initialJson = JsonDocument.Parse(await initialResponse.Content.ReadAsStringAsync()).RootElement;
        initialJson.GetProperty("personalDiscountPercent").GetDecimal().Should().Be(0m);

        using (var scope = Factory.Services.CreateScope())
        {
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await publishEndpoint.Publish(new UserDiscountUpdatedEvent
            {
                UserId = userId,
                PersonalDiscountPercent = 15m
            });
        }

        bool processed = false;
        for (int i = 0; i < 50; i++)
        {
            using (var scope = Factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var loyalty = await context.UserLoyalties.FirstOrDefaultAsync(l => l.UserId == userId);
                if (loyalty != null && loyalty.PersonalDiscountPercent == 15m)
                {
                    processed = true;
                    break;
                }
            }
            await Task.Delay(100);
        }

        processed.Should().BeTrue();

        var updatedResponse = await Client.GetAsync($"/venue/loyalty/{userId}");
        updatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedJson = JsonDocument.Parse(await updatedResponse.Content.ReadAsStringAsync()).RootElement;
        updatedJson.GetProperty("personalDiscountPercent").GetDecimal().Should().Be(15m);
    }
}
