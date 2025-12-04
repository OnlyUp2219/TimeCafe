namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class GetVisitHistoryTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetVisitHistory_Should_Return200_WhenUserHasVisits()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedVisitAsync("user123", isActive: false);
        await SeedVisitAsync("user123", isActive: false);
        await SeedVisitAsync("user123", isActive: true);

        var response = await Client.GetAsync("/visits/history/user123?pageNumber=1&pageSize=10");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("visits", out var visits).Should().BeTrue();
            visits.ValueKind.Should().Be(JsonValueKind.Array);
            visits.GetArrayLength().Should().BeGreaterThanOrEqualTo(3);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetVisitHistory_Should_Return200_WhenUserHasVisits] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetVisitHistory_Should_Return200_WhenUserHasNoVisits()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.GetAsync("/visits/history/user123?pageNumber=1&pageSize=10");
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
            Console.WriteLine($"[Endpoint_GetVisitHistory_Should_Return200_WhenUserHasNoVisits] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetVisitHistory_Should_RespectPagination_WhenCalled()
    {
        await ClearDatabaseAndCacheAsync();
        for (int i = 0; i < 5; i++)
        {
            await SeedVisitAsync("user123", isActive: false);
        }

        var response = await Client.GetAsync("/visits/history/user123?pageNumber=1&pageSize=2");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var visits = json.GetProperty("visits");
            visits.GetArrayLength().Should().BeLessThanOrEqualTo(2);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetVisitHistory_Should_RespectPagination_WhenCalled] Response: {jsonString}");
            throw;
        }
    }
}
