namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class DeleteVisitTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteVisit_Should_Return204_WhenVisitExists()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId);

        var response = await Client.DeleteAsync($"/venue/visits/{visit.VisitId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Endpoint_DeleteVisit_Should_Return404_WhenVisitNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.DeleteAsync($"/venue/visits/{TestData.NonExistingIds.NonExistingVisitIdString}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteVisit_Should_Return404_WhenVisitNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteVisit_Should_ActuallyRemoveFromDatabase_WhenDeleted()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit2UserId);

        var deleteResponse = await Client.DeleteAsync($"/venue/visits/{visit.VisitId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/venue/visits/{visit.VisitId}");
        var jsonString = await getResponse.Content.ReadAsStringAsync();
        try
        {
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteVisit_Should_ActuallyRemoveFromDatabase_WhenDeleted] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteVisit_Should_Return422_WhenVisitIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.DeleteAsync($"/venue/visits/{Guid.Empty}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteVisit_Should_Return422_WhenVisitIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteVisit_Should_NotAffectOtherVisits_WhenOneIsDeleted()
    {
        await ClearDatabaseAndCacheAsync();
        var visit1 = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId);
        var visit2 = await SeedVisitAsync(TestData.NewVisits.NewVisit2UserId);

        await Client.DeleteAsync($"/venue/visits/{visit1.VisitId}");

        var response = await Client.GetAsync($"/venue/visits/{visit2.VisitId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("visitId").GetGuid().Should().Be(visit2.VisitId);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteVisit_Should_NotAffectOtherVisits_WhenOneIsDeleted] Response: {jsonString}");
            throw;
        }
    }
}







