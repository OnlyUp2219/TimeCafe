namespace UserProfile.TimeCafe.Test.Integration.Endpoints.AdditionalInfosEndpoints;

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

        await Client.PostAsJsonAsync("/userprofile/infos", createDto1);
        await Client.PostAsJsonAsync("/userprofile/infos", createDto2);

        // Act
        var response = await Client.GetAsync($"/userprofile/profiles/{userId}/infos");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var items = json.GetProperty("items");
            items.ValueKind.Should().Be(JsonValueKind.Array);
            var itemsList = items.EnumerateArray().ToList();
            itemsList.Should().HaveCount(2);
            itemsList[0].TryGetProperty("infoText", out var text1).Should().BeTrue();
            itemsList[1].TryGetProperty("infoText", out var text2).Should().BeTrue();
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
        var response = await Client.GetAsync($"/userprofile/profiles/{userId}/infos");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var items = json.GetProperty("items");
            items.ValueKind.Should().Be(JsonValueKind.Array);
            var itemsList = items.EnumerateArray().ToList();
            itemsList.Should().BeEmpty();
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
        var nonExistentUserId = TestData.NonExistingUsers.UserId1;

        // Act
        var response = await Client.GetAsync($"/userprofile/profiles/{nonExistentUserId}/infos");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(jsonString).RootElement;
        var items = json.GetProperty("items");
        items.ValueKind.Should().Be(JsonValueKind.Array);
        var itemsList = items.EnumerateArray().ToList();
        itemsList.Should().BeEmpty();
    }
}
