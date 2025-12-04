namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class GetActiveVisitByUserTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetActiveVisitByUser_Should_Return200_WhenUserHasActiveVisit()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync("user123", isActive: true);

        var response = await Client.GetAsync("/visits/active/user123");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("visit", out var visitJson).Should().BeTrue();
            visitJson.GetProperty("userId").GetString().Should().Be("user123");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActiveVisitByUser_Should_Return200_WhenUserHasActiveVisit] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetActiveVisitByUser_Should_Return404_WhenUserHasNoActiveVisit()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.GetAsync("/visits/active/user123");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActiveVisitByUser_Should_Return404_WhenUserHasNoActiveVisit] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetActiveVisitByUser_Should_ReturnOnlyActiveVisit_WhenUserHasMultipleVisits()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedVisitAsync("user123", isActive: false);
        var activeVisit = await SeedVisitAsync("user123", isActive: true);

        var response = await Client.GetAsync("/visits/active/user123");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var visit = json.GetProperty("visit");
            visit.GetProperty("visitId").GetInt32().Should().Be(activeVisit.VisitId);
            visit.GetProperty("status").GetInt32().Should().Be(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActiveVisitByUser_Should_ReturnOnlyActiveVisit_WhenUserHasMultipleVisits] Response: {jsonString}");
            throw;
        }
    }
}
