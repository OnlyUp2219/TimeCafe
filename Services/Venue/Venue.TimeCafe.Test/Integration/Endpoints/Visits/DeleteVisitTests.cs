namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class DeleteVisitTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteVisit_Should_Return200_WhenVisitExists()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync("user1");

        var response = await Client.DeleteAsync($"/visits/{visit.VisitId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteVisit_Should_Return200_WhenVisitExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteVisit_Should_Return404_WhenVisitNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.DeleteAsync("/visits/9999");
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
        var visit = await SeedVisitAsync("user1");

        var deleteResponse = await Client.DeleteAsync($"/visits/{visit.VisitId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await Client.GetAsync($"/visits/{visit.VisitId}");
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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Endpoint_DeleteVisit_Should_Return422_WhenVisitIdIsInvalid(int invalidId)
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.DeleteAsync($"/visits/{invalidId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteVisit_Should_Return422_WhenVisitIdIsInvalid({invalidId})] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteVisit_Should_NotAffectOtherVisits_WhenOneIsDeleted()
    {
        await ClearDatabaseAndCacheAsync();
        var visit1 = await SeedVisitAsync("user1");
        var visit2 = await SeedVisitAsync("user2");

        await Client.DeleteAsync($"/visits/{visit1.VisitId}");

        var response = await Client.GetAsync($"/visits/{visit2.VisitId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteVisit_Should_NotAffectOtherVisits_WhenOneIsDeleted] Response: {jsonString}");
            throw;
        }
    }
}
