using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints;

public class DeleteProfileTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteProfile_Should_Return200_WhenProfileExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, "Петр", "Иванов");

        // Act
        var response = await Client.DeleteAsync($"/profiles/{userId}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString()!.Should().Contain("удалён");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteProfile_Should_Return200_WhenProfileExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteProfile_Should_Return404_WhenProfileNotFound()
    {
        // Act
        var randomGuid = Guid.NewGuid();
        var response = await Client.DeleteAsync($"/profiles/{randomGuid}");
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
            Console.WriteLine($"[Endpoint_DeleteProfile_Should_Return404_WhenProfileNotFound] Response: {jsonString}");
            throw;
        }
    }
}
