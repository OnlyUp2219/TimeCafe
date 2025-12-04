namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class HasActiveVisitTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_HasActiveVisit_Should_ReturnTrue_WhenUserHasActiveVisit()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedVisitAsync("user123", isActive: true);

        var response = await Client.GetAsync("/visits/has-active/user123");
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
        await SeedVisitAsync("user123", isActive: false);

        var response = await Client.GetAsync("/visits/has-active/user123");
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

        var response = await Client.GetAsync("/visits/has-active/user123");
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
