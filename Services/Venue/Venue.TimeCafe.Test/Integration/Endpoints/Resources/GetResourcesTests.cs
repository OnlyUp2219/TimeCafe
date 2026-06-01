using System.Net;
using System.Text.Json;
using FluentAssertions;
using Venue.TimeCafe.Application.CQRS.Resources.DTOs;
using Venue.TimeCafe.Test.Integration.Helpers;
using Xunit;

namespace Venue.TimeCafe.Test.Integration.Endpoints.Resources;

public class GetResourcesTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetResources_Should_ReturnAllResources()
    {
        await ClearDatabaseAndCacheAsync();

        var group = await SeedResourceGroupAsync("Main Hall", 20);
        var r1 = await SeedResourceAsync(group.ResourceGroupId, "Table 1", 4);
        var r2 = await SeedResourceAsync(group.ResourceGroupId, "Table 2", 6);

        var response = await Client.GetAsync("/venue/resources");
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = JsonSerializer.Deserialize<List<ResourceDto>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.Should().NotBeNull();
            result!.Count.Should().BeGreaterThanOrEqualTo(2);
            result.Any(x => x.ResourceId == r1.ResourceId).Should().BeTrue();
            result.Any(x => x.ResourceId == r2.ResourceId).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetResources_Should_ReturnAllResources] Response: {jsonString}");
            throw;
        }
    }
}
