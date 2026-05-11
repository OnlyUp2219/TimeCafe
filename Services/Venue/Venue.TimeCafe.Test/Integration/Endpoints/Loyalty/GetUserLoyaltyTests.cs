namespace Venue.TimeCafe.Test.Integration.Endpoints.Loyalty;

public class GetUserLoyaltyTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetUserLoyalty_Should_ReturnLoyalty_WhenDiscountUpdated()
    {
        // Arrange
        await ClearDatabaseAndCacheAsync();
        
        // Clear UserLoyalties specifically as it's not in ClearDatabaseAndCacheAsync
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.UserLoyalties.RemoveRange(context.UserLoyalties);
            await context.SaveChangesAsync();
        }

        var userId = Guid.NewGuid();
        var discountPercent = 10m;

        // Publish event to MassTransit (In-Memory)
        using (var scope = Factory.Services.CreateScope())
        {
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await publishEndpoint.Publish(new UserDiscountUpdatedEvent
            {
                UserId = userId,
                PersonalDiscountPercent = discountPercent
            });
        }

        // Wait for the consumer to process the message (In-Memory is fast but async)
        bool processed = false;
        for (int i = 0; i < 10; i++)
        {
            using (var scope = Factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var loyalty = await context.UserLoyalties.FirstOrDefaultAsync(l => l.UserId == userId);
                if (loyalty != null && loyalty.PersonalDiscountPercent == discountPercent)
                {
                    processed = true;
                    break;
                }
            }
            await Task.Delay(100);
        }

        processed.Should().BeTrue("Consumer should process the message and update DB");

        // Act
        var response = await Client.GetAsync($"/venue/loyalty/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(jsonString).RootElement;

        json.GetProperty("userId").GetString().Should().Be(userId.ToString());
        json.GetProperty("personalDiscountPercent").GetDecimal().Should().Be(discountPercent);
    }

    [Fact]
    public async Task Endpoint_GetUserLoyalty_Should_ReturnZeroDiscount_WhenNotFound()
    {
        // Arrange
        await ClearDatabaseAndCacheAsync();
        var userId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/venue/loyalty/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(jsonString).RootElement;

        json.GetProperty("userId").GetString().Should().Be(userId.ToString());
        json.GetProperty("personalDiscountPercent").GetDecimal().Should().Be(0m);
    }
}
