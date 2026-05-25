namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class ApproveVisitTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_ApproveVisit_Should_Return200_WhenVisitPending()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Pending);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/approve", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("status").GetInt32().Should().Be((int)VisitStatus.Active);
            json.TryGetProperty("approvedByUserId", out _).Should().BeTrue();
            json.TryGetProperty("approvedAt", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ApproveVisit_Should_Return200_WhenVisitPending] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ApproveVisit_Should_Return404_WhenVisitNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync($"/venue/visits/{TestData.NonExistingIds.NonExistingVisitIdString}/approve", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ApproveVisit_Should_Return404_WhenVisitNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ApproveVisit_Should_Return422_WhenVisitIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync($"/venue/visits/{Guid.Empty}/approve", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ApproveVisit_Should_Return422_WhenVisitIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ApproveVisit_Should_Return409_WhenVisitAlreadyApproved()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Approved);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/approve", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ApproveVisit_Should_Return409_WhenVisitAlreadyApproved] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ApproveVisit_Should_Return409_WhenVisitAlreadyRejected()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Rejected);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/approve", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ApproveVisit_Should_Return409_WhenVisitAlreadyRejected] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ApproveVisit_Should_Return409_WhenVisitAlreadyCompleted()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Completed);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/approve", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ApproveVisit_Should_Return409_WhenVisitAlreadyCompleted] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ApproveVisit_Should_Return409_WhenVisitAlreadyCancelled()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Cancelled);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/approve", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ApproveVisit_Should_Return409_WhenVisitAlreadyCancelled] Response: {jsonString}");
            throw;
        }
    }
}
