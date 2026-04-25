namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class GetactiveVisitsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetactiveVisits_Should_Return200_WhenactiveVisitsExist()
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
            var visits = json;
            visits.ValueKind.Should().Be(JsonValueKind.Array);
            visits.GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetactiveVisits_Should_Return200_WhenactiveVisitsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetactiveVisits_Should_Return200_WhenNoactiveVisitsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId, isActive: false);

        var response = await Client.GetAsync("/venue/visits/active");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var visits = json;
            visits.ValueKind.Should().Be(JsonValueKind.Array);
            visits.GetArrayLength().Should().Be(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetactiveVisits_Should_Return200_WhenNoactiveVisitsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetactiveVisits_Should_OnlyReturnactiveVisits_WhenCalled()
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
            var visits = json;
            foreach (var visit in visits.EnumerateArray())
            {
                visit.GetProperty("status").GetInt32().Should().Be((int)VisitStatus.Active);
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetactiveVisits_Should_OnlyReturnactiveVisits_WhenCalled] Response: {jsonString}");
            throw;
        }
    }
}














