namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class LoginV2Tests : BaseEndpointTest
{
    public LoginV2Tests(IntegrationApiFactory factory) : base(factory)
    {
        SeedUser("v2_unconfirmed@example.com", "password123", false);
        SeedUser("v2_confirmed@example.com", "password123", true);
    }

    [Fact]
    public async Task Endpoint_LoginV2_Should_ReturnEmailConfirmedFalse_WhenEmailNotConfirmed()
    {
        // Arrange
        var dto = new { Email = "v2_unconfirmed@example.com", Password = "password123" };

        // Act
        var response = await Client.PostAsJsonAsync("/login-jwt-v2", dto);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(body).RootElement;
            json.TryGetProperty("emailConfirmed", out var emailConfirmed).Should().BeTrue();
            emailConfirmed.GetBoolean().Should().BeFalse();
            json.TryGetProperty("accessToken", out var _).Should().BeFalse();
            json.TryGetProperty("refreshToken", out var _).Should().BeFalse();
            response.Headers.TryGetValues("Set-Cookie", out var setCookies).Should().BeFalse();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_LoginV2_Should_ReturnEmailConfirmedFalse_WhenEmailNotConfirmed] Response: {body}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_LoginV2_Should_SetRefreshCookieAndReturnAccess_WhenEmailConfirmed()
    {
        // Arrange
        var dto = new { Email = "v2_confirmed@example.com", Password = "password123" };

        // Act
        var response = await Client.PostAsJsonAsync("/login-jwt-v2", dto);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(body).RootElement;
            json.TryGetProperty("emailConfirmed", out var emailConfirmed).Should().BeTrue();
            emailConfirmed.GetBoolean().Should().BeTrue();
            json.TryGetProperty("accessToken", out var accessToken).Should().BeTrue();
            accessToken.GetString()!.Should().NotBeNullOrWhiteSpace();
            json.TryGetProperty("refreshToken", out var _).Should().BeFalse();
            response.Headers.TryGetValues("Set-Cookie", out var setCookies).Should().BeTrue();
            setCookies!.Any(c => c.StartsWith("refresh_token=")).Should().BeTrue();
            var refreshCookie = setCookies!.First(c => c.StartsWith("refresh_token="));
            var refreshCookieLower = refreshCookie.ToLowerInvariant();
            refreshCookieLower.Should().Contain("httponly");
            refreshCookieLower.Should().Contain("secure");
            refreshCookieLower.Should().Contain("samesite=none");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_LoginV2_Should_SetRefreshCookieAndReturnAccess_WhenEmailConfirmed] Response: {body}");
            throw;
        }
    }
}