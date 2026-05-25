namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class RejectVisitTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_RejectVisit_Should_Return200_WhenVisitPending()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Pending);
        var payload = new { reason = "Нет свободных мест" };

        var response = await Client.PostAsJsonAsync($"/venue/visits/{visit.VisitId}/reject", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("status").GetInt32().Should().Be((int)VisitStatus.Rejected);
            json.GetProperty("rejectionReason").GetString().Should().Be("Нет свободных мест");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_RejectVisit_Should_Return200_WhenVisitPending] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_RejectVisit_Should_Return404_WhenVisitNotFound()
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new { reason = "Причина" };

        var response = await Client.PostAsJsonAsync($"/venue/visits/{TestData.NonExistingIds.NonExistingVisitIdString}/reject", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_RejectVisit_Should_Return404_WhenVisitNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_RejectVisit_Should_Return422_WhenVisitIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new { reason = "Причина" };

        var response = await Client.PostAsJsonAsync($"/venue/visits/{Guid.Empty}/reject", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_RejectVisit_Should_Return422_WhenVisitIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_RejectVisit_Should_Return422_WhenReasonIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Pending);
        var payload = new { reason = "" };

        var response = await Client.PostAsJsonAsync($"/venue/visits/{visit.VisitId}/reject", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_RejectVisit_Should_Return422_WhenReasonIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_RejectVisit_Should_Return422_WhenReasonExceeds500Chars()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Pending);
        var payload = new { reason = new string('x', 501) };

        var response = await Client.PostAsJsonAsync($"/venue/visits/{visit.VisitId}/reject", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_RejectVisit_Should_Return422_WhenReasonExceeds500Chars] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_RejectVisit_Should_Return409_WhenVisitAlreadyRejected()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Rejected);
        var payload = new { reason = "Другая причина" };

        var response = await Client.PostAsJsonAsync($"/venue/visits/{visit.VisitId}/reject", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_RejectVisit_Should_Return409_WhenVisitAlreadyRejected] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_RejectVisit_Should_Return409_WhenVisitAlreadyActive()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Active);
        var payload = new { reason = "Причина" };

        var response = await Client.PostAsJsonAsync($"/venue/visits/{visit.VisitId}/reject", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_RejectVisit_Should_Return409_WhenVisitAlreadyActive] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_RejectVisit_Should_Return409_WhenVisitAlreadyCompleted()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId, status: VisitStatus.Completed);
        var payload = new { reason = "Причина" };

        var response = await Client.PostAsJsonAsync($"/venue/visits/{visit.VisitId}/reject", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_RejectVisit_Should_Return409_WhenVisitAlreadyCompleted] Response: {jsonString}");
            throw;
        }
    }
}
