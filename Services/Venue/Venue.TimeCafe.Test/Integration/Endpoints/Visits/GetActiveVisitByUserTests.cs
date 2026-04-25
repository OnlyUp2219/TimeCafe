namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class GetactiveVisitByUserTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetactiveVisitByUser_Should_Return200_WhenUserHasActiveVisit()
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

            json.GetProperty("userId").GetString().Should().Be(userId.ToString());
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetactiveVisitByUser_Should_Return200_WhenUserHasActiveVisit] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetactiveVisitByUser_Should_Return404_WhenUserHasNoactiveVisit()
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
            Console.WriteLine($"[Endpoint_GetactiveVisitByUser_Should_Return404_WhenUserHasNoactiveVisit] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetactiveVisitByUser_Should_ReturnOnlyactiveVisit_WhenUserHasMultipleVisits()
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
            var visit = json;
            json.GetProperty("visitId").GetString().Should().Be(activeVisit.VisitId.ToString());
            json.GetProperty("status").GetInt32().Should().Be((int)VisitStatus.Active);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetactiveVisitByUser_Should_ReturnOnlyactiveVisit_WhenUserHasMultipleVisits] Response: {jsonString}");
            throw;
        }
    }
}













