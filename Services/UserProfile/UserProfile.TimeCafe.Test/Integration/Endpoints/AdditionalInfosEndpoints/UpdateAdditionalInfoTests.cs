namespace UserProfile.TimeCafe.Test.Integration.Endpoints.AdditionalInfosEndpoints;

public class UpdateAdditionalInfoTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_UpdateAdditionalInfo_Should_Return204_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, TestData.TestProfiles.TestFirstName, TestData.TestProfiles.TestLastName);

        // Create info first
        var createDto = new { userId = userId.ToString(), infoText = TestData.TestInfoTexts.OriginalInfo, createdBy = (string?)null };
        var createResponse = await Client.PostAsJsonAsync("/userprofile/infos", createDto);
        createResponse.EnsureSuccessStatusCode();
        var createJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync()).RootElement;
        var infoId = createJson.GetProperty("infoId").GetString();

        // Update info
        var updateDto = new { userId = userId.ToString(), infoText = TestData.TestInfoTexts.UpdatedInfo, createdBy = (string?)null };

        // Act
        var response = await Client.PutAsJsonAsync($"/userprofile/infos/{infoId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify
        var getResponse = await Client.GetAsync($"/userprofile/infos/{infoId}");
        var json = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync()).RootElement;
        json.GetProperty("infoText").GetString()!.Should().Be(TestData.TestInfoTexts.UpdatedInfo);
    }

    [Fact]
    public async Task Endpoint_UpdateAdditionalInfo_Should_Return404_WhenNotFound()
    {
        // Arrange
        var nonExistentInfoId = TestData.NonExistingUsers.UserId1;
        var dto = new { userId = TestData.ExistingUsers.User1Id, infoText = TestData.AdditionalInfoData.UpdatedInfoText, createdBy = (string?)null };

        // Act
        var response = await Client.PutAsJsonAsync($"/userprofile/infos/{nonExistentInfoId}", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        if (!string.IsNullOrWhiteSpace(jsonString))
        {
            var json = JsonDocument.Parse(jsonString).RootElement;
            if (json.TryGetProperty("code", out var code))
                code.GetString()!.Should().Be("AdditionalInfoNotFound");
        }
    }

    [Fact]
    public async Task Endpoint_UpdateAdditionalInfo_Should_Return422_WhenEmptyText()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, TestData.TestProfiles.TestFirstName, TestData.TestProfiles.TestLastName);

        var createDto = new { userId = userId.ToString(), infoText = TestData.TestInfoTexts.OriginalInfo, createdBy = (string?)null };
        var createResponse = await Client.PostAsJsonAsync("/userprofile/infos", createDto);
        var createJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync()).RootElement;
        var infoId = createJson.GetProperty("infoId").GetString();

        var updateDto = new { userId = userId.ToString(), infoText = "", createdBy = (string?)null };

        // Act
        var response = await Client.PutAsJsonAsync($"/userprofile/infos/{infoId}", updateDto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var json = JsonDocument.Parse(jsonString).RootElement;
        json.TryGetProperty("code", out var code).Should().BeTrue();
        code.GetString()!.Should().Be("ValidationError");
    }
}


