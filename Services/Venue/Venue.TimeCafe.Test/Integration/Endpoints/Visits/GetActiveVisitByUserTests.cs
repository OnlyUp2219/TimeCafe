namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class GetActiveVisitByUserTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetActiveVisitByUser_Should_Return200_WhenUserHasActiveVisit()
    {
        await ClearDatabaseAndCacheAsync();
        var userId = TestData.NewVisits.NewVisit1UserId;
        var visit = await SeedVisitAsync(userId, isActive: true);

        var response = await Client.GetAsync($"/venue/visits/active/{userId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("visit", out var visitJson).Should().BeTrue();
            visitJson.GetProperty("userId").GetString().Should().Be(userId.ToString());
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

        var userId = TestData.NonExistingIds.NonExistingUserId;
        var response = await Client.GetAsync($"/venue/visits/active/{userId}");
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
        var userId = TestData.NewVisits.NewVisit2UserId;
        await SeedVisitAsync(userId, isActive: false);
        var activeVisit = await SeedVisitAsync(userId, isActive: true);

        var response = await Client.GetAsync($"/venue/visits/active/{userId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var visit = json.GetProperty("visit");
            visit.GetProperty("visitId").GetString().Should().Be(activeVisit.VisitId.ToString());
            visit.GetProperty("status").GetInt32().Should().Be((int)VisitStatus.Active);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActiveVisitByUser_Should_ReturnOnlyActiveVisit_WhenUserHasMultipleVisits] Response: {jsonString}");
            throw;
        }
    }
}
