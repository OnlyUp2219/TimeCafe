using System.Net;
using FluentAssertions;
using Venue.TimeCafe.Test.Integration.Helpers;
using Xunit;

namespace Venue.TimeCafe.Test.Integration.Endpoints.Resources;

public class DeleteResourceTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteResource_Should_Return200_WhenExists()
    {
        await ClearDatabaseAndCacheAsync();

        var group = await SeedResourceGroupAsync("Main Hall", 20);
        var resource = await SeedResourceAsync(group.ResourceGroupId, "Table 1", 4);

        var response = await Client.DeleteAsync($"/venue/resources/{resource.ResourceId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Проверяем, что стол действительно удален
        var getResponse = await Client.GetAsync($"/venue/resources/{resource.ResourceId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
