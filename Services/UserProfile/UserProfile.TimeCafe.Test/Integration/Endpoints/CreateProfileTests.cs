using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints;

public class CreateProfileTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CreateProfile_Should_Return201_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new { userId = userId.ToString(), firstName = "Анна", lastName = "Иванова", gender = (byte)Gender.Female };

        // Act
        var response = await Client.PostAsJsonAsync("/profiles", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString()!.Should().NotBeNullOrWhiteSpace();
            json.TryGetProperty("profile", out var profile).Should().BeTrue();
            profile.ValueKind.Should().Be(JsonValueKind.Object);
            Guid.Parse(profile.GetProperty("userId").GetString()!).Should().Be(userId);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateProfile_Should_Return201_WhenValid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateProfile_Should_Return422_WhenValidationFails()
    {
        // Arrange
        var dto = new { userId = Guid.NewGuid().ToString(), firstName = "", lastName = "", gender = (byte)Gender.NotSpecified };

        // Act
        var response = await Client.PostAsJsonAsync("/profiles", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");
            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.ValueKind.Should().Be(JsonValueKind.Array);
            errors.GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateProfile_Should_Return422_WhenValidationFails] Response: {jsonString}");
            throw;
        }
    }
}
