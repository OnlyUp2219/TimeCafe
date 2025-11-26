using UserProfile.TimeCafe.Domain.Models;
using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints;

public class CreateProfileTests : BaseEndpointTest
{
    public CreateProfileTests(IntegrationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Endpoint_CreateProfile_Should_Return201_WhenValid()
    {
        // Arrange
        var dto = new { userId = "newuser", firstName = "Анна", lastName = "Иванова", gender = (byte)Gender.Female };

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
            profile.GetProperty("userId").GetString()!.Should().Be("newuser");
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
        var dto = new { userId = "", firstName = "", lastName = "", gender = (byte)Gender.NotSpecified };

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
            errors.GetArrayLength().Should().BeGreaterThan(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateProfile_Should_Return422_WhenValidationFails] Response: {jsonString}");
            throw;
        }
    }
}
