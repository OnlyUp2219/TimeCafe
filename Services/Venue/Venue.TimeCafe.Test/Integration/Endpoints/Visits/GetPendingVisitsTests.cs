namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class GetPendingVisitsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetPendingVisits_Should_Return200_WithPendingVisitsOnly()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Pending);
        await SeedVisitAsync(TestData.NewVisits.NewVisit2UserId, status: VisitStatus.Pending);
        await SeedVisitAsync(TestData.ExistingVisits.Visit1UserId, status: VisitStatus.Active);
        await SeedVisitAsync(TestData.ExistingVisits.Visit2UserId, status: VisitStatus.Completed);

        var response = await Client.GetAsync("/venue/visits/pending?page=1&pageSize=20");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var items = json.GetProperty("items");
            items.GetArrayLength().Should().Be(2);
            foreach (var item in items.EnumerateArray())
            {
                item.GetProperty("status").GetInt32().Should().Be((int)VisitStatus.Pending);
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetPendingVisits_Should_Return200_WithPendingVisitsOnly] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetPendingVisits_Should_Return200_WithEmptyList_WhenNoPending()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Active);
        await SeedVisitAsync(TestData.NewVisits.NewVisit2UserId, status: VisitStatus.Completed);

        var response = await Client.GetAsync("/venue/visits/pending?page=1&pageSize=20");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("items").GetArrayLength().Should().Be(0);
            json.GetProperty("metadata").GetProperty("totalCount").GetInt32().Should().Be(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetPendingVisits_Should_Return200_WithEmptyList_WhenNoPending] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetPendingVisits_Should_ReturnCorrectPaginationMetadata()
    {
        await ClearDatabaseAndCacheAsync();
        for (int i = 0; i < 5; i++)
        {
            await SeedVisitAsync(Guid.NewGuid(), status: VisitStatus.Pending);
        }

        var response = await Client.GetAsync("/venue/visits/pending?page=1&pageSize=2");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("items").GetArrayLength().Should().Be(2);
            var metadata = json.GetProperty("metadata");
            metadata.GetProperty("page").GetInt32().Should().Be(1);
            metadata.GetProperty("pageSize").GetInt32().Should().Be(2);
            metadata.GetProperty("totalCount").GetInt32().Should().Be(5);
            metadata.GetProperty("totalPages").GetInt32().Should().Be(3);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetPendingVisits_Should_ReturnCorrectPaginationMetadata] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetPendingVisits_Should_ReturnEmpty_WhenPageExceedsTotal()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Pending);

        var response = await Client.GetAsync("/venue/visits/pending?page=100&pageSize=20");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("items").GetArrayLength().Should().Be(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetPendingVisits_Should_ReturnEmpty_WhenPageExceedsTotal] Response: {jsonString}");
            throw;
        }
    }
}
