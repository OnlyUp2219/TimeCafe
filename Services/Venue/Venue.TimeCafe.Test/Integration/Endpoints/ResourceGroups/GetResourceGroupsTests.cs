using System.Net;
using System.Text.Json;
using FluentAssertions;
using Venue.TimeCafe.Application.CQRS.ResourceGroups.DTOs;
using Venue.TimeCafe.Test.Integration.Helpers;
using Xunit;

namespace Venue.TimeCafe.Test.Integration.Endpoints.ResourceGroups;

public class GetResourceGroupsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetResourceGroups_Should_ReturnAllGroups()
    {
        await ClearDatabaseAndCacheAsync();

        var g1 = await SeedResourceGroupAsync("Zone A", 10);
        var g2 = await SeedResourceGroupAsync("Zone B", 20);

        var response = await Client.GetAsync("/venue/resource-groups");
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = JsonSerializer.Deserialize<List<ResourceGroupDto>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.Should().NotBeNull();
            result!.Count.Should().BeGreaterThanOrEqualTo(2);
            result.Any(x => x.ResourceGroupId == g1.ResourceGroupId).Should().BeTrue();
            result.Any(x => x.ResourceGroupId == g2.ResourceGroupId).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetResourceGroups_Should_ReturnAllGroups] Response: {jsonString}");
            throw;
        }
    }
}
