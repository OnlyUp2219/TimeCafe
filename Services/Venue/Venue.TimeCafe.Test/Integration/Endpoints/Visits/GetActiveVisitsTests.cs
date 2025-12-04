namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class GetActiveVisitsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetActiveVisits_Should_Return200_WhenActiveVisitsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedVisitAsync("user1", isActive: true);
        await SeedVisitAsync("user2", isActive: true);
        await SeedVisitAsync("user3", isActive: false);

        var response = await Client.GetAsync("/visits/active");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("visits", out var visits).Should().BeTrue();
            visits.ValueKind.Should().Be(JsonValueKind.Array);
            visits.GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActiveVisits_Should_Return200_WhenActiveVisitsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetActiveVisits_Should_Return200_WhenNoActiveVisitsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedVisitAsync("user1", isActive: false);

        var response = await Client.GetAsync("/visits/active");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("visits", out var visits).Should().BeTrue();
            visits.ValueKind.Should().Be(JsonValueKind.Array);
            visits.GetArrayLength().Should().Be(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActiveVisits_Should_Return200_WhenNoActiveVisitsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetActiveVisits_Should_OnlyReturnActiveVisits_WhenCalled()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedVisitAsync("user1", isActive: true);
        await SeedVisitAsync("user2", isActive: false);

        var response = await Client.GetAsync("/visits/active");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var visits = json.GetProperty("visits");
            foreach (var visit in visits.EnumerateArray())
            {
                visit.GetProperty("status").GetInt32().Should().Be(0);
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActiveVisits_Should_OnlyReturnActiveVisits_WhenCalled] Response: {jsonString}");
            throw;
        }
    }
}
