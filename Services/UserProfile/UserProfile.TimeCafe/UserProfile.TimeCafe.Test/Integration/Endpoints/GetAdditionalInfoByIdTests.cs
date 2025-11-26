using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints;

public class GetAdditionalInfoByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetAdditionalInfoById_Should_Return200_WhenExists()
    {
        // Arrange
        await SeedProfileAsync("user001", "Тест", "Юзер");
        var createDto = new { userId = "user001", infoText = "Тестовая информация", createdBy = (string?)null };
        var createResponse = await Client.PostAsJsonAsync("/infos", createDto);
        createResponse.EnsureSuccessStatusCode();
        var createJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync()).RootElement;
        var infoId = createJson.GetProperty("info").GetProperty("infoId").GetInt32();

        // Act
        var response = await Client.GetAsync($"/infos/{infoId}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("infoId", out var id).Should().BeTrue();
            id.GetInt32().Should().Be(infoId);
            json.TryGetProperty("infoText", out var text).Should().BeTrue();
            text.GetString()!.Should().Be("Тестовая информация");
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
        var response = await Client.GetAsync("/infos/99999");
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
