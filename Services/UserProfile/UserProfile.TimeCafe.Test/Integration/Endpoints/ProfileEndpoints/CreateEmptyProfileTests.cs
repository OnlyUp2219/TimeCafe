using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints;

public class CreateEmptyProfileTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CreateEmptyProfile_Should_Return201_WhenValid()
    {
        // Arrange
        var userId = TestData.NewProfiles.NewUser1Id;

        // Act
        var response = await Client.PostAsync($"/profiles/empty/{userId}", null);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString()!.ToLower().Should().Contain("профиль");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateEmptyProfile_Should_Return201_WhenValid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateEmptyProfile_Should_Return409_WhenAlreadyExists()
    {
        // Arrange - User1 уже существует
        var userId = TestData.ExistingUsers.User1Id;

        // Act
        var response = await Client.PostAsync($"/profiles/empty/{userId}", null);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ProfileAlreadyExists");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateEmptyProfile_Should_Return409_WhenAlreadyExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateEmptyProfile_Should_Return422_WhenInvalidGuid()
    {
        // Arrange
        var invalidId = TestData.InvalidIds.NotAGuid;

        // Act
        var response = await Client.PostAsync($"/profiles/empty/{invalidId}", null);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity); 
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateEmptyProfile_Should_Return400_WhenInvalidGuid] Response: {jsonString}");
            throw;
        }
    }
}
