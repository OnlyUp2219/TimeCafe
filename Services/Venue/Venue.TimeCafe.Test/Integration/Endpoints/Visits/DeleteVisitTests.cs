namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class DeleteVisitTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteVisit_Should_Return200_WhenVisitExists()
    {
        await ClearDatabaseAndCacheAsync();
        var visit = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId);

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

        var response = await Client.DeleteAsync($"/visits/{TestData.NonExistingIds.NonExistingVisitIdString}");
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
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Endpoint_DeleteVisit_Should_Return422_WhenVisitIdIsInvalid(string invalidId)
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
        var visit1 = await SeedVisitAsync(TestData.NewVisits.NewVisit1UserId);
        var visit2 = await SeedVisitAsync(TestData.NewVisits.NewVisit2UserId);

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
