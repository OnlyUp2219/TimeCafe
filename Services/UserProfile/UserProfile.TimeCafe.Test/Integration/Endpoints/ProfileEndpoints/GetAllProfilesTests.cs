namespace UserProfile.TimeCafe.Test.Integration.Endpoints.ProfileEndpoints;

public class GetAllProfilesTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetAllProfiles_Should_Return200_WhenProfilesExist()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        await SeedProfileAsync(userId1, TestData.ExistingUsers.User1FirstName, TestData.ExistingUsers.User1LastName);
        await SeedProfileAsync(userId2, TestData.ExistingUsers.User2FirstName, TestData.ExistingUsers.User2LastName);

        // Act
        var response = await Client.GetAsync("/profiles");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.ValueKind.Should().Be(JsonValueKind.Array);
            json.GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllProfiles_Should_Return200_WhenProfilesExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetAllProfiles_Should_Return200_WhenNoProfiles()
    {
        // Act
        var response = await Client.GetAsync("/profiles");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.ValueKind.Should().Be(JsonValueKind.Array);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllProfiles_Should_Return200_WhenNoProfiles] Response: {jsonString}");
            throw;
        }
    }
}
