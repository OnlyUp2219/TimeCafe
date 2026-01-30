namespace UserProfile.TimeCafe.Test.Integration.Endpoints.AdditionalInfosEndpoints;

public class CreateAdditionalInfoTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CreateAdditionalInfo_Should_Return201_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, TestData.TestProfiles.TestFirstName, TestData.TestProfiles.TestLastName);
        var dto = new { userId = userId.ToString(), infoText = TestData.TestInfoTexts.NewInfo, createdBy = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync("/userprofile/infos", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("info", out var info).Should().BeTrue();
            info.TryGetProperty("infoId", out var infoId).Should().BeTrue();
            infoId.GetString().Should().NotBeNullOrEmpty();
            info.TryGetProperty("infoText", out var text).Should().BeTrue();
            text.GetString()!.Should().Be(TestData.TestInfoTexts.NewInfo);
            info.TryGetProperty("userId", out var uid).Should().BeTrue();
            uid.GetString()!.Should().Be(userId.ToString());
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateAdditionalInfo_Should_Return201_WhenValid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateAdditionalInfo_Should_Return404_WhenProfileNotFound()
    {
        // Arrange
        var nonExistentUserId = TestData.NonExistingUsers.UserId1;
        var dto = new { userId = nonExistentUserId, infoText = TestData.AdditionalInfoData.NewInfoText, createdBy = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync("/userprofile/infos", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var json = JsonDocument.Parse(jsonString).RootElement;
        json.TryGetProperty("code", out var code).Should().BeTrue();
        code.GetString()!.Should().Be("ProfileNotFound");
    }

    [Fact]
    public async Task Endpoint_CreateAdditionalInfo_Should_Return422_WhenEmptyText()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, TestData.TestProfiles.TestFirstName, TestData.TestProfiles.TestLastName);
        var dto = new { userId = userId.ToString(), infoText = "", createdBy = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync("/userprofile/infos", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateAdditionalInfo_Should_Return422_WhenEmptyText] Response: {jsonString}");
            throw;
        }
    }
}
