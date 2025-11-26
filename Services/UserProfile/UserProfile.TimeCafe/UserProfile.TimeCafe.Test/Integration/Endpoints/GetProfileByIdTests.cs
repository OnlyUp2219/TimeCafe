using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints;

public class GetProfileByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetProfileById_Should_Return200_WhenProfileExists()
    {
        // Arrange
        await SeedProfileAsync("user123", "Иван", "Петров");

        // Act
        var response = await Client.GetAsync("/profiles/user123");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("userId", out var userId).Should().BeTrue();
            userId.GetString()!.Should().Be("user123");
            json.TryGetProperty("firstName", out var firstName).Should().BeTrue();
            firstName.GetString()!.Should().Be("Иван");
            json.TryGetProperty("lastName", out var lastName).Should().BeTrue();
            lastName.GetString()!.Should().Be("Петров");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetProfileById_Should_Return200_WhenProfileExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetProfileById_Should_Return404_WhenProfileNotFound()
    {
        // Act
        var response = await Client.GetAsync("/profiles/unknown");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ProfileNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetProfileById_Should_Return404_WhenProfileNotFound] Response: {jsonString}");
            throw;
        }
    }
}
