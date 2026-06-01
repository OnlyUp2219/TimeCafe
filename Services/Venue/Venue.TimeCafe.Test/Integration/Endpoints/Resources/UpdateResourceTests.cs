using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Venue.TimeCafe.Application.CQRS.Resources.DTOs;
using Venue.TimeCafe.Test.Integration.Helpers;
using Xunit;

namespace Venue.TimeCafe.Test.Integration.Endpoints.Resources;

public class UpdateResourceTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_UpdateResource_Should_Return200_WhenRequestIsValid()
    {
        await ClearDatabaseAndCacheAsync();

        var group = await SeedResourceGroupAsync("Main Hall", 20);
        var group2 = await SeedResourceGroupAsync("Coworking", 10);
        var resource = await SeedResourceAsync(group.ResourceGroupId, "Old Table", 4);

        var request = new
        {
            ResourceId = resource.ResourceId,
            ResourceGroupId = group2.ResourceGroupId,
            Name = "New Table",
            Capacity = 6,
            IsActive = true
        };

        var response = await Client.PutAsJsonAsync($"/venue/resources/{resource.ResourceId}", request);
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = JsonSerializer.Deserialize<ResourceDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.Should().NotBeNull();
            result!.ResourceId.Should().Be(resource.ResourceId);
            result.Name.Should().Be(request.Name);
            result.Capacity.Should().Be(request.Capacity);
            result.ResourceGroupId.Should().Be(group2.ResourceGroupId);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateResource_Should_Return200_WhenRequestIsValid] Response: {jsonString}");
            throw;
        }
    }
}
