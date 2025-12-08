using FluentAssertions;
using System.Net;
using System.Text.Json;
using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints;

public class UpdateAdditionalInfoTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_UpdateAdditionalInfo_Should_Return200_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, "Тест", "Юзер");

        // Create info first
        var createDto = new { userId = userId.ToString(), infoText = "Исходная информация", createdBy = (string?)null };
        var createResponse = await Client.PostAsJsonAsync("/infos", createDto);
        createResponse.EnsureSuccessStatusCode();
        var createJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync()).RootElement;
        var infoId = createJson.GetProperty("info").GetProperty("infoId").GetString();

        // Update info
        var updateDto = new { infoId = infoId, userId = userId.ToString(), infoText = "Обновлённая информация", createdBy = (string?)null };

        // Act
        var response = await Client.PutAsJsonAsync("/infos", updateDto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("info", out var info).Should().BeTrue();
            info.TryGetProperty("infoText", out var text).Should().BeTrue();
            text.GetString()!.Should().Be("Обновлённая информация");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateAdditionalInfo_Should_Return200_WhenValid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdateAdditionalInfo_Should_Return404_WhenNotFound()
    {
        // Arrange
        var nonExistentInfoId = TestData.NonExistingUsers.UserId1;
        var dto = new { infoId = nonExistentInfoId, userId = TestData.ExistingUsers.User1Id, infoText = TestData.AdditionalInfoData.UpdatedInfoText, createdBy = (string?)null };

        // Act
        var response = await Client.PutAsJsonAsync("/infos", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var json = JsonDocument.Parse(jsonString).RootElement;
        json.TryGetProperty("code", out var code).Should().BeTrue();
        code.GetString()!.Should().Be("AdditionalInfoNotFound");
    }

    [Fact]
    public async Task Endpoint_UpdateAdditionalInfo_Should_Return422_WhenEmptyText()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, "Тест", "Юзер");

        var createDto = new { userId = userId.ToString(), infoText = "Исходная информация", createdBy = (string?)null };
        var createResponse = await Client.PostAsJsonAsync("/infos", createDto);
        var createJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync()).RootElement;
        var infoId = createJson.GetProperty("info").GetProperty("infoId").GetString();

        var updateDto = new { infoId = infoId, userId = userId.ToString(), infoText = "", createdBy = (string?)null };

        // Act
        var response = await Client.PutAsJsonAsync("/infos", updateDto);
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
            Console.WriteLine($"[Endpoint_UpdateAdditionalInfo_Should_Return422_WhenEmptyText] Response: {jsonString}");
            throw;
        }
    }
}
