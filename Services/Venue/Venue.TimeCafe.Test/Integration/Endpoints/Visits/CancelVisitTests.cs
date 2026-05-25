namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class CancelVisitTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CancelVisit_Should_Return200_WhenVisitPending()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(Guid.Parse(TestAuthHandler.TestUserId), status: VisitStatus.Pending);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/cancel", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CancelVisit_Should_Return200_WhenVisitPending] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CancelVisit_Should_Return404_WhenVisitNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync($"/venue/visits/{TestData.NonExistingIds.NonExistingVisitIdString}/cancel", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CancelVisit_Should_Return404_WhenVisitNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CancelVisit_Should_Return422_WhenVisitIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync($"/venue/visits/{Guid.Empty}/cancel", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CancelVisit_Should_Return422_WhenVisitIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CancelVisit_Should_Return409_WhenVisitAlreadyActive()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(Guid.Parse(TestAuthHandler.TestUserId), status: VisitStatus.Active);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/cancel", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CancelVisit_Should_Return409_WhenVisitAlreadyActive] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CancelVisit_Should_Return409_WhenVisitAlreadyCancelled()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(Guid.Parse(TestAuthHandler.TestUserId), status: VisitStatus.Cancelled);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/cancel", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CancelVisit_Should_Return409_WhenVisitAlreadyCancelled] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CancelVisit_Should_Return409_WhenVisitAlreadyCompleted()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(Guid.Parse(TestAuthHandler.TestUserId), status: VisitStatus.Completed);

        var response = await Client.PostAsync($"/venue/visits/{visit.VisitId}/cancel", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CancelVisit_Should_Return409_WhenVisitAlreadyCompleted] Response: {jsonString}");
            throw;
        }
    }
}
