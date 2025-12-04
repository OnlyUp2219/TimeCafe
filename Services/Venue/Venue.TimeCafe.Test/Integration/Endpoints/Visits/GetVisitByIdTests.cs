namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class GetVisitByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetVisitById_Should_Return200_WhenVisitExists()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync("user1");

        var response = await Client.GetAsync($"/visits/{visit.VisitId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("visit", out var visitJson).Should().BeTrue();
            visitJson.GetProperty("visitId").GetInt32().Should().Be(visit.VisitId);
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

        var response = await Client.GetAsync("/visits/9999");
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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task Endpoint_GetVisitById_Should_Return422_WhenVisitIdIsInvalid(int invalidId)
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.GetAsync($"/visits/{invalidId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetVisitById_Should_Return422_WhenVisitIdIsInvalid({invalidId})] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetVisitById_Should_ReturnAllProperties_WhenVisitExists()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync("user1");

        var response = await Client.GetAsync($"/visits/{visit.VisitId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var visitJson = json.GetProperty("visit");
            visitJson.GetProperty("visitId").GetInt32().Should().Be(visit.VisitId);
            visitJson.GetProperty("userId").GetString().Should().Be("user1");
            visitJson.TryGetProperty("entryTime", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetVisitById_Should_ReturnAllProperties_WhenVisitExists] Response: {jsonString}");
            throw;
        }
    }
}
