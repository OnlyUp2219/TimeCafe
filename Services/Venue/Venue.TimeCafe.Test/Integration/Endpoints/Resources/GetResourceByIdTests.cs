using System.Net;
using System.Text.Json;
using FluentAssertions;
using Venue.TimeCafe.Application.CQRS.Resources.DTOs;
using Venue.TimeCafe.Test.Integration.Helpers;
using Xunit;

namespace Venue.TimeCafe.Test.Integration.Endpoints.Resources;

public class GetResourceByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetResourceById_Should_Return200_WhenExists()
    {
        await ClearDatabaseAndCacheAsync();

        var group = await SeedResourceGroupAsync("Main Hall", 20);
        var resource = await SeedResourceAsync(group.ResourceGroupId, "Table 1", 4);

        var response = await Client.GetAsync($"/venue/resources/{resource.ResourceId}");
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = JsonSerializer.Deserialize<ResourceDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.Should().NotBeNull();
            result!.ResourceId.Should().Be(resource.ResourceId);
            result.Name.Should().Be("Table 1");
            result.Capacity.Should().Be(4);
            result.ResourceGroupId.Should().Be(group.ResourceGroupId);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetResourceById_Should_Return200_WhenExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetResourceById_Should_Return404_WhenNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var nonExistingId = Guid.NewGuid();
        var response = await Client.GetAsync($"/venue/resources/{nonExistingId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
