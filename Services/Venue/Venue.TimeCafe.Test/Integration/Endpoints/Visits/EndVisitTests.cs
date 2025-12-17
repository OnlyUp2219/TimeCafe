namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class EndVisitTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_EndVisit_Should_Return200_WhenVisitExists()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, isActive: true);
        var payload = new { visitId = visit.VisitId };

        var response = await Client.PostAsJsonAsync("/visits/end", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
            json.TryGetProperty("visit", out _).Should().BeTrue();
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
        var payload = new { visitId = TestData.NonExistingIds.NonExistingVisitIdString };

        var response = await Client.PostAsJsonAsync("/visits/end", payload);
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

    [Theory]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Endpoint_EndVisit_Should_Return422_WhenVisitIdIsInvalid(string invalidId)
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new { visitId = invalidId };

        var response = await Client.PostAsJsonAsync("/visits/end", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_EndVisit_Should_Return422_WhenVisitIdIsInvalid({invalidId})] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_EndVisit_Should_CalculateCost_WhenVisitEnded()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit2UserId, isActive: true);
        var payload = new { visitId = visit.VisitId };

        var response = await Client.PostAsJsonAsync("/visits/end", payload);
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
