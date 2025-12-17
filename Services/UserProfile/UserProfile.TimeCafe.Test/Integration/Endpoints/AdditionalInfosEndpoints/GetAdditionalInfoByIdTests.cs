using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints.AdditionalInfosEndpoints;

public class GetAdditionalInfoByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetAdditionalInfoById_Should_Return200_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, TestData.TestProfiles.TestFirstName, TestData.TestProfiles.TestLastName);
        var createDto = new { userId = userId.ToString(), infoText = TestData.TestInfoTexts.TestInfo, createdBy = (string?)null };
        var createResponse = await Client.PostAsJsonAsync("/infos", createDto);
        createResponse.EnsureSuccessStatusCode();
        var createJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync()).RootElement;
        var infoId = createJson.GetProperty("info").GetProperty("infoId").GetString();

        // Act
        var response = await Client.GetAsync($"/infos/{infoId}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("infoId", out var id).Should().BeTrue();
            id.GetString()!.Should().Be(infoId);
            json.TryGetProperty("infoText", out var text).Should().BeTrue();
            text.GetString()!.Should().Be(TestData.TestInfoTexts.TestInfo);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAdditionalInfoById_Should_Return200_WhenExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetAdditionalInfoById_Should_Return404_WhenNotFound()
    {
        // Act
        var randomGuid = Guid.NewGuid();
        var response = await Client.GetAsync($"/infos/{randomGuid}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("AdditionalInfoNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAdditionalInfoById_Should_Return404_WhenNotFound] Response: {jsonString}");
            throw;
        }
    }
}
