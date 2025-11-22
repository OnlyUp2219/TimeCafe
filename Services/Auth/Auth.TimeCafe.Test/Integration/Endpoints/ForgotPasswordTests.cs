using System.Net;
using System.Text.Json;

namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class ForgotPasswordTests : BaseEndpointTest
{
    private const string Endpoint = "/forgot-password-link-mock";

    public ForgotPasswordTests(IntegrationApiFactory factory) : base(factory)
    {
        SeedUser("exists@example.com", "P@ssw0rd!", true);
    }

    private HttpClient CreateClientWithDisabledRateLimiter()
    {
        var overrides = new Dictionary<string, string?>
        {
            ["RateLimiter:EmailSms:MinIntervalSeconds"] = "0",
            ["RateLimiter:EmailSms:MaxRequests"] = "10000"
        };

        return Factory.WithWebHostBuilder(b => b.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(overrides)))
                      .CreateClient();
    }

    [Fact]
    public async Task ForgotPassword_Mock_Should_ReturnMockCallback_WhenUserExists()
    {
        // Arrange
        var dto = new { Email = "exists@example.com" };
        using var client = CreateClientWithDisabledRateLimiter();

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var json = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(json).RootElement;
        doc.TryGetProperty("callbackUrl", out _).Should().BeTrue();
        doc.GetProperty("callbackUrl").GetString().Should().Contain("/resetPassword?email=exists@example.com&code=");
    }

    [Fact]
    public async Task ForgotPassword_Mock_Should_ReturnSuccessSilent_WhenUserNotExists()
    {
        // Arrange
        var dto = new { Email = "notexists@example.com" };
        using var client = CreateClientWithDisabledRateLimiter();

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var json = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(json).RootElement;
        doc.TryGetProperty("message", out var msg).Should().BeTrue();
        msg.GetString().Should().Be("Если пользователь существует, письмо отправлено");
    }

    [Theory]
    [InlineData("", "Email обязателен")]
    [InlineData("invalid", "Некорректный формат email")]
    public async Task ForgotPassword_Mock_Should_ReturnValidationError_WhenInvalidEmail(string email, string expectedPart)
    {
        // Arrange
        var dto = new { Email = email };
        using var client = CreateClientWithDisabledRateLimiter();

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var json = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var doc = JsonDocument.Parse(json).RootElement;
        doc.GetProperty("code").GetString().Should().Be("ValidationError");
        doc.GetProperty("errors").EnumerateArray()
           .Any(e => e.GetProperty("message").GetString()!.Contains(expectedPart))
           .Should().BeTrue();
    }
}