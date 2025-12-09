using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints;

public class GetAdditionalInfosByUserIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetAdditionalInfosByUserId_Should_Return200_WithMultipleInfos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, TestData.TestProfiles.TestFirstName, TestData.TestProfiles.TestLastName);

        // Create multiple infos
        var createDto1 = new { userId = userId.ToString(), infoText = TestData.TestInfoTexts.FirstInfo, createdBy = (string?)null };
        var createDto2 = new { userId = userId.ToString(), infoText = TestData.TestInfoTexts.SecondInfo, createdBy = (string?)null };

        await Client.PostAsJsonAsync("/infos", createDto1);
        await Client.PostAsJsonAsync("/infos", createDto2);

        // Act
        var response = await Client.GetAsync($"/profiles/{userId}/infos");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.ValueKind.Should().Be(JsonValueKind.Array);
            var items = json.EnumerateArray().ToList();
            items.Should().HaveCount(2);
            items[0].TryGetProperty("infoText", out var text1).Should().BeTrue();
            items[1].TryGetProperty("infoText", out var text2).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAdditionalInfosByUserId_Should_Return200_WithMultipleInfos] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetAdditionalInfosByUserId_Should_Return200_WithEmptyArray()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, TestData.TestProfiles.TestFirstName, TestData.TestProfiles.TestLastName);

        // Act
        var response = await Client.GetAsync($"/profiles/{userId}/infos");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.ValueKind.Should().Be(JsonValueKind.Array);
            var items = json.EnumerateArray().ToList();
            items.Should().BeEmpty();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAdditionalInfosByUserId_Should_Return200_WithEmptyArray] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetAdditionalInfosByUserId_Should_Return404_WhenProfileNotFound()
    {
        // Arrange
        var nonExistentUserId = NonExistingUsers.UserId1;

        // Act
        var response = await Client.GetAsync($"/profiles/{nonExistentUserId}/infos");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var json = JsonDocument.Parse(jsonString).RootElement;
        json.TryGetProperty("code", out var code).Should().BeTrue();
        code.GetString()!.Should().Be("ProfileNotFound");
    }
}
