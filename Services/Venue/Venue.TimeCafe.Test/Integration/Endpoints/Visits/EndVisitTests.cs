namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class EndVisitTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_EndVisit_Should_Return200_WhenVisitExists()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, isActive: true);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/end", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;


            json.TryGetProperty("calculatedCost", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_EndVisit_Should_Return200_WhenVisitExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_EndVisit_Should_Return404_WhenVisitNotFound()
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new { };

        var response = await Client.PostAsync($"/venue/visits/{TestData.NonExistingIds.NonExistingVisitIdString}/end", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_EndVisit_Should_Return404_WhenVisitNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_EndVisit_Should_Return422_WhenVisitIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync($"/venue/visits/{Guid.Empty}/end", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_EndVisit_Should_Return422_WhenVisitIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_EndVisit_Should_CalculateCost_WhenVisitEnded()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit2UserId, isActive: true);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/end", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var calculatedCost = json.GetProperty("calculatedCost").GetDecimal();
            calculatedCost.Should().BeGreaterThan(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_EndVisit_Should_CalculateCost_WhenVisitEnded] Response: {jsonString}");
            throw;
        }
    }
}













