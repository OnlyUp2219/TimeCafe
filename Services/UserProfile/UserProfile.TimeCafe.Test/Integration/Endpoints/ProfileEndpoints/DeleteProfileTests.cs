using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints;

public class DeleteProfileTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteProfile_Should_Return200_WhenProfileExists()
    {
        // Arrange
        var userId = TestData.ExistingUsers.User3Id;

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

            await SeedProfileAsync(userId, TestData.ExistingUsers.User3FirstName,
                                 TestData.ExistingUsers.User3LastName,
                                 TestData.ExistingUsers.User3Gender);
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
        // Arrange
        var userId = TestData.NonExistingUsers.UserId1;

        // Act
        var response = await Client.DeleteAsync($"/profiles/{userId}");
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

    [Fact]
    public async Task Endpoint_DeleteProfile_Should_Return422_WhenInvalidGuid()
    {
        // Arrange
        var invalidId = TestData.InvalidIds.NotAGuid;

        // Act
        var response = await Client.DeleteAsync($"/profiles/{invalidId}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteProfile_Should_Return400_WhenInvalidGuid] Response: {jsonString}");
            throw;
        }
    }
}
