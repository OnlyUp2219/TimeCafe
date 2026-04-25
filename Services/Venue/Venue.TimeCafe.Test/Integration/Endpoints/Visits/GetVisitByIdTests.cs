namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class GetVisitByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetVisitById_Should_Return200_WhenVisitExists()
    {
        await ClearDatabaseAndCacheAsync();
        var userId = TestData.NewVisits.NewVisit1UserId;
        var visit = await SeedVisitAsync(userId);

        var response = await Client.GetAsync($"/venue/visits/{visit.VisitId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;

            json.GetProperty("visitId").GetString().Should().Be(visit.VisitId.ToString());
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetVisitById_Should_Return200_WhenVisitExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetVisitById_Should_Return404_WhenVisitNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var nonExistingId = TestData.NonExistingIds.NonExistingVisitId;
        var response = await Client.GetAsync($"/venue/visits/{nonExistingId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetVisitById_Should_Return404_WhenVisitNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetVisitById_Should_Return422_WhenVisitIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.GetAsync($"/venue/visits/{Guid.Empty}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetVisitById_Should_Return422_WhenVisitIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetVisitById_Should_ReturnAllProperties_WhenVisitExists()
    {
        await ClearDatabaseAndCacheAsync();
        var userId = TestData.NewVisits.NewVisit2UserId;
        var visit = await SeedVisitAsync(userId);

        var response = await Client.GetAsync($"/venue/visits/{visit.VisitId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;

            json.GetProperty("visitId").GetString().Should().Be(visit.VisitId.ToString());
            json.GetProperty("userId").GetString().Should().Be(userId.ToString());
            json.TryGetProperty("entryTime", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetVisitById_Should_ReturnAllProperties_WhenVisitExists] Response: {jsonString}");
            throw;
        }
    }
}













