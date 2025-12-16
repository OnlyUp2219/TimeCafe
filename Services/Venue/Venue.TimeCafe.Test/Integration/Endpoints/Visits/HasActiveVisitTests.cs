namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class HasActiveVisitTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_HasActiveVisit_Should_ReturnTrue_WhenUserHasActiveVisit()
    {
        await ClearDatabaseAndCacheAsync();
        var userId = TestData.NewVisits.NewVisit1UserId;
        await SeedVisitAsync(userId, isActive: true);

        var response = await Client.GetAsync($"/visits/has-active/{userId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("hasActiveVisit", out var hasActive).Should().BeTrue();
            hasActive.GetBoolean().Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_HasActiveVisit_Should_ReturnTrue_WhenUserHasActiveVisit] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_HasActiveVisit_Should_ReturnFalse_WhenUserHasNoActiveVisit()
    {
        await ClearDatabaseAndCacheAsync();
        var userId = TestData.NewVisits.NewVisit1UserId;
        await SeedVisitAsync(userId, isActive: false);

        var response = await Client.GetAsync($"/visits/has-active/{userId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("hasActiveVisit", out var hasActive).Should().BeTrue();
            hasActive.GetBoolean().Should().BeFalse();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_HasActiveVisit_Should_ReturnFalse_WhenUserHasNoActiveVisit] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_HasActiveVisit_Should_ReturnFalse_WhenUserHasNoVisits()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.GetAsync($"/visits/has-active/{TestData.DefaultValues.DefaultUserId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("hasActiveVisit", out var hasActive).Should().BeTrue();
            hasActive.GetBoolean().Should().BeFalse();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_HasActiveVisit_Should_ReturnFalse_WhenUserHasNoVisits] Response: {jsonString}");
            throw;
        }
    }
}
