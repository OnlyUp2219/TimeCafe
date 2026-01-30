namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class GetActiveVisitsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetActiveVisits_Should_Return200_WhenActiveVisitsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, isActive: true);
        await SeedVisitAsync(TestData.NewVisits.NewVisit2UserId, isActive: true);
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId, isActive: false);

        var response = await Client.GetAsync("/venue/visits/active");
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
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId, isActive: false);

        var response = await Client.GetAsync("/venue/visits/active");
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
        await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, isActive: true);
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId, isActive: false);

        var response = await Client.GetAsync("/venue/visits/active");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var visits = json.GetProperty("visits");
            foreach (var visit in visits.EnumerateArray())
            {
                visit.GetProperty("status").GetInt32().Should().Be((int)VisitStatus.Active);
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActiveVisits_Should_OnlyReturnActiveVisits_WhenCalled] Response: {jsonString}");
            throw;
        }
    }
}
