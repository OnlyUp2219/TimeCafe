namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class LogoutTests : BaseEndpointTest
{
    public LogoutTests(IntegrationApiFactory factory) : base(factory)
    {
        SeedUser("confirmed@example.com", "password123", true);
    }

    [Fact]
    public async Task Endpoint_Logout_Should_ReturnOkAndRevoke_WhenCookieValid()
    {
        // Arrange
        var loginDto = new { Email = "confirmed@example.com", Password = "password123" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt-v2", loginDto);
        var loginBody = await loginResp.Content.ReadAsStringAsync();

        // Act
        var refreshToken = ExtractCookieValue(loginResp, "refresh_token");
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/logout");
        logoutRequest.Headers.Add("Cookie", $"refresh_token={refreshToken}");
        var response = await Client.SendAsync(logoutRequest);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(body).RootElement;
            json.TryGetProperty("revoked", out var revoked).Should().BeTrue();
            revoked.GetBoolean().Should().BeTrue();
            response.Headers.TryGetValues("Set-Cookie", out var setCookies).Should().BeTrue();
            setCookies!.Any(c => c.StartsWith("refresh_token=", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Logout_Should_ReturnOkAndRevoke_WhenCookieValid] login: {loginBody} response: {body}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Logout_Should_ReturnOkWithRevokedFalse_WhenCookieMissing()
    {
        // Arrange
        var response = await Client.PostAsync("/logout", new StringContent(string.Empty));
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(body).RootElement;
            json.TryGetProperty("revoked", out var revoked).Should().BeTrue();
            revoked.GetBoolean().Should().BeFalse();
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString()!.Should().Contain("отсутствует");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Logout_Should_ReturnOkWithRevokedFalse_WhenCookieMissing] response: {body}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Logout_Should_ReturnOkWithRevokedFalse_WhenCookieAlreadyRevoked()
    {
        // Arrange
        var loginDto = new { Email = "confirmed@example.com", Password = "password123" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt-v2", loginDto);
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshToken = ExtractCookieValue(loginResp, "refresh_token");
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/logout");
        logoutRequest.Headers.Add("Cookie", $"refresh_token={refreshToken}");
        (await Client.SendAsync(logoutRequest)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var secondRequest = new HttpRequestMessage(HttpMethod.Post, "/logout");
        secondRequest.Headers.Add("Cookie", $"refresh_token={refreshToken}");
        var secondResponse = await Client.SendAsync(secondRequest);
        var body = await secondResponse.Content.ReadAsStringAsync();

        // Assert
        try
        {
            secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(body).RootElement;
            json.TryGetProperty("revoked", out var revoked).Should().BeTrue();
            revoked.GetBoolean().Should().BeFalse();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Logout_Should_ReturnOkWithRevokedFalse_WhenCookieAlreadyRevoked] response: {body}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Logout_Should_ReturnOkWithRevokedFalse_WhenCookieMalformed()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/logout");
        request.Headers.Add("Cookie", "refresh_token=!!!@@@###");

        // Act
        var response = await Client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(body).RootElement;
            json.TryGetProperty("revoked", out var revoked).Should().BeTrue();
            revoked.GetBoolean().Should().BeFalse();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Logout_Should_ReturnOkWithRevokedFalse_WhenCookieMalformed] response: {body}");
            throw;
        }
    }

    private static string ExtractCookieValue(HttpResponseMessage response, string cookieName)
    {
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        var raw = cookies!.First(c => c.StartsWith(cookieName + "=", StringComparison.OrdinalIgnoreCase));
        return raw.Split(';', 2)[0].Substring(cookieName.Length + 1);
    }
}
