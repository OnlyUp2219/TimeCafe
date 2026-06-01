using System.Net;
using System.Text.Json;
using FluentAssertions;
using Venue.TimeCafe.Application.CQRS.ResourceGroups.DTOs;
using Venue.TimeCafe.Test.Integration.Helpers;
using Xunit;

namespace Venue.TimeCafe.Test.Integration.Endpoints.ResourceGroups;

public class GetResourceGroupByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetResourceGroupById_Should_Return200_WhenExists()
    {
        await ClearDatabaseAndCacheAsync();

        var group = await SeedResourceGroupAsync("VIP Zone", 8);

        var response = await Client.GetAsync($"/venue/resource-groups/{group.ResourceGroupId}");
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = JsonSerializer.Deserialize<ResourceGroupDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.Should().NotBeNull();
            result!.ResourceGroupId.Should().Be(group.ResourceGroupId);
            result.Name.Should().Be("VIP Zone");
            result.Capacity.Should().Be(8);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetResourceGroupById_Should_Return200_WhenExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetResourceGroupById_Should_Return404_WhenNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var nonExistingId = Guid.NewGuid();
        var response = await Client.GetAsync($"/venue/resource-groups/{nonExistingId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
