namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class GetVisitHistoryTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetVisitHistory_Should_Return200_WhenUserHasVisits()
    {
        await ClearDatabaseAndCacheAsync();
        var userId = TestData.NewVisits.NewVisit1UserId;
        await SeedVisitAsync(userId, isActive: false);
        await SeedVisitAsync(userId, isActive: false);
        await SeedVisitAsync(userId, isActive: true);

        var response = await Client.GetAsync($"/venue/visits/history/{userId}?pageNumber={TestData.DefaultValues.FirstPage}&pageSize={TestData.DefaultValues.DefaultPageSize}");
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

        var response = await Client.GetAsync($"/venue/visits/history/{TestData.DefaultValues.DefaultUserId}?pageNumber={TestData.DefaultValues.FirstPage}&pageSize={TestData.DefaultValues.DefaultPageSize}");
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
        var userId = TestData.NewVisits.NewVisit2UserId;
        for (int i = 0; i < 5; i++)
        {
            await SeedVisitAsync(userId, isActive: false);
        }

        var response = await Client.GetAsync($"/venue/visits/history/{userId}?pageNumber={TestData.DefaultValues.FirstPage}&pageSize=2");
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
