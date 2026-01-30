namespace UserProfile.TimeCafe.Test.Integration.Endpoints.ProfileEndpoints;

public class GetProfileByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetProfileById_Should_Return200_WhenProfileExists()
    {
        // Arrange - данные уже загружены в InitializeAsync
        var userId = TestData.ExistingUsers.User1Id;

        // Act
        var response = await Client.GetAsync($"/userprofile/profiles/{userId}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("userId", out var userIdProp).Should().BeTrue();
            userIdProp.GetString()!.Should().Be(userId);
            json.TryGetProperty("firstName", out var firstName).Should().BeTrue();
            firstName.GetString()!.Should().Be(TestData.ExistingUsers.User1FirstName);
            json.TryGetProperty("lastName", out var lastName).Should().BeTrue();
            lastName.GetString()!.Should().Be(TestData.ExistingUsers.User1LastName);
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
        // Arrange
        var userId = TestData.NonExistingUsers.UserId1;

        // Act
        var response = await Client.GetAsync($"/userprofile/profiles/{userId}");
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

    [Fact]
    public async Task Endpoint_GetProfileById_Should_Return422_WhenInvalidGuid()
    {
        // Arrange
        var invalidId = TestData.InvalidIds.NotAGuid;

        // Act
        var response = await Client.GetAsync($"/userprofile/profiles/{invalidId}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetProfileById_Should_Return400_WhenInvalidGuid] Response: {jsonString}");
            throw;
        }
    }
}
