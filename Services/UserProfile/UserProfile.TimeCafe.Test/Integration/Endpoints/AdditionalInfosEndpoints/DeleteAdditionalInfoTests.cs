namespace UserProfile.TimeCafe.Test.Integration.Endpoints.AdditionalInfosEndpoints;

public class DeleteAdditionalInfoTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteAdditionalInfo_Should_Return200_WhenValid()
    {
        // Arrange
        var infoId = TestData.AdditionalInfoData.Info1Id;

        // Act
        var response = await Client.DeleteAsync($"/infos/{infoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_DeleteAdditionalInfo_Should_Return404_WhenNotFound()
    {
        // Arrange
        var nonExistentInfoId = TestData.NonExistingUsers.UserId1;

        // Act
        var response = await Client.DeleteAsync($"/infos/{nonExistentInfoId}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var json = JsonDocument.Parse(jsonString).RootElement;
        json.TryGetProperty("code", out var code).Should().BeTrue();
        code.GetString()!.Should().Be("AdditionalInfoNotFound");
    }

    [Fact]
    public async Task Endpoint_DeleteAdditionalInfo_Should_Return404_WhenInvalidGuid()
    {
        // Arrange 
        var invalidGuid = TestData.InvalidIds.NotAGuid;

        // Act
        var response = await Client.DeleteAsync($"/infos/{invalidGuid}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}