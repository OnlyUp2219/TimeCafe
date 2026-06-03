namespace Venue.TimeCafe.Test.Integration.Endpoints.ResourceGroups;

public class CreateResourceGroupTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CreateResourceGroup_Should_Return201_WhenRequestIsValid()
    {
        await ClearDatabaseAndCacheAsync();

        var request = new
        {
            Name = "Vip Zone",
            Description = "VIP area with consoles",
            Capacity = 12,
            IsActive = true
        };

        var response = await Client.PostAsJsonAsync("/venue/resource-groups", request);
        var jsonString = await response.Content.ReadAsStringAsync();

        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = JsonSerializer.Deserialize<ResourceGroupDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            result.Should().NotBeNull();
            result!.Name.Should().Be(request.Name);
            result.Description.Should().Be(request.Description);
            result.Capacity.Should().Be(request.Capacity);
            result.IsActive.Should().BeTrue();
            result.ResourceGroupId.Should().NotBeEmpty();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateResourceGroup_Should_Return201_WhenRequestIsValid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateResourceGroup_Should_Return422_WhenNameIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();

        var request = new
        {
            Name = "",
            Description = "Description",
            Capacity = 5,
            IsActive = true
        };

        var response = await Client.PostAsJsonAsync("/venue/resource-groups", request);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
