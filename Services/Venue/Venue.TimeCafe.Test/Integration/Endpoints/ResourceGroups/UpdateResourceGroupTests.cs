using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Venue.TimeCafe.Application.CQRS.ResourceGroups.DTOs;
using Venue.TimeCafe.Test.Integration.Helpers;
using Xunit;

namespace Venue.TimeCafe.Test.Integration.Endpoints.ResourceGroups;

public class UpdateResourceGroupTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_UpdateResourceGroup_Should_Return200_WhenRequestIsValid()
    {
        await ClearDatabaseAndCacheAsync();

        var group = await SeedResourceGroupAsync("Old Name", 10);

        var request = new
        {
            ResourceGroupId = group.ResourceGroupId,
            Name = "New Name",
            Description = "Updated Description",
            Capacity = 15,
            IsActive = true
        };

        var response = await Client.PutAsJsonAsync($"/venue/resource-groups/{group.ResourceGroupId}", request);
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = JsonSerializer.Deserialize<ResourceGroupDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.Should().NotBeNull();
            result!.ResourceGroupId.Should().Be(group.ResourceGroupId);
            result.Name.Should().Be(request.Name);
            result.Description.Should().Be(request.Description);
            result.Capacity.Should().Be(request.Capacity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateResourceGroup_Should_Return200_WhenRequestIsValid] Response: {jsonString}");
            throw;
        }
    }
}
