using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Venue.TimeCafe.Application.CQRS.Resources.DTOs;
using Venue.TimeCafe.Test.Integration.Helpers;
using Xunit;

namespace Venue.TimeCafe.Test.Integration.Endpoints.Resources;

public class CreateResourceTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CreateResource_Should_Return200_WhenRequestIsValid()
    {
        await ClearDatabaseAndCacheAsync();

        var group = await SeedResourceGroupAsync("Main Hall", 20);

        var request = new
        {
            ResourceGroupId = group.ResourceGroupId,
            Name = "Table 1",
            Capacity = 4,
            IsActive = true
        };

        var response = await Client.PostAsJsonAsync("/venue/resources", request);
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = JsonSerializer.Deserialize<ResourceDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.Should().NotBeNull();
            result!.Name.Should().Be(request.Name);
            result.Capacity.Should().Be(request.Capacity);
            result.ResourceGroupId.Should().Be(group.ResourceGroupId);
            result.IsActive.Should().BeTrue();
            result.ResourceId.Should().NotBeEmpty();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateResource_Should_Return200_WhenRequestIsValid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateResource_Should_Return400_WhenGroupNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var nonExistingGroupId = Guid.NewGuid();

        var request = new
        {
            ResourceGroupId = nonExistingGroupId,
            Name = "Table 1",
            Capacity = 4,
            IsActive = true
        };

        var response = await Client.PostAsJsonAsync("/venue/resources", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
