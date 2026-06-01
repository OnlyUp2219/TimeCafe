using System.Net;
using FluentAssertions;
using Venue.TimeCafe.Test.Integration.Helpers;
using Xunit;

namespace Venue.TimeCafe.Test.Integration.Endpoints.ResourceGroups;

public class DeleteResourceGroupTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteResourceGroup_Should_Return200_WhenExists()
    {
        await ClearDatabaseAndCacheAsync();

        var group = await SeedResourceGroupAsync("To Delete", 5);

        var response = await Client.DeleteAsync($"/venue/resource-groups/{group.ResourceGroupId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Проверяем, что группа действительно удалена
        var getResponse = await Client.GetAsync($"/venue/resource-groups/{group.ResourceGroupId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
