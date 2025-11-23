namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class RefreshTokenV2Tests : BaseEndpointTest
{
    public RefreshTokenV2Tests(IntegrationApiFactory factory) : base(factory)
    {
        SeedUser("v2_refresh@example.com", "P@ssw0rd!", true);
    }

    private static string ExtractCookieValue(IEnumerable<string> setCookies, string name)
    {
        var raw = setCookies.First(c => c.StartsWith(name + "="));
        var firstPart = raw.Split(';', 2)[0];
        return firstPart.Substring(name.Length + 1);
    }

    [Fact]
    public async Task Endpoint_RefreshV2_Should_Return401_WhenCookieMissing()
    {
        // Arrange
        var request = new StringContent(string.Empty);

        // Act
        var response = await Client.PostAsync("/refresh-jwt-v2", request);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_RefreshV2_Should_Return401_WhenCookieMissing] Response: {body}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_RefreshV2_Should_RotateCookieAndReturnAccess_WhenCookieValid()
    {
        // Arrange
        var loginDto = new { Email = "v2_refresh@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt-v2", loginDto);
        loginResp.EnsureSuccessStatusCode();
        loginResp.Headers.TryGetValues("Set-Cookie", out var initialCookies).Should().BeTrue();
        var oldRefresh = ExtractCookieValue(initialCookies!, "refresh_token");

        // Act
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/refresh-jwt-v2");
        refreshRequest.Headers.Add("Cookie", $"refresh_token={oldRefresh}");
        var refreshResp = await Client.SendAsync(refreshRequest);
        var body = await refreshResp.Content.ReadAsStringAsync();

        // Assert
        try
        {
            refreshResp.StatusCode.Should().Be(HttpStatusCode.OK);
            refreshResp.Headers.TryGetValues("Set-Cookie", out var newCookies).Should().BeTrue();
            var newRefresh = ExtractCookieValue(newCookies!, "refresh_token");
            newRefresh.Should().NotBe(oldRefresh);
            var json = JsonDocument.Parse(body).RootElement;
            json.TryGetProperty("accessToken", out var accessProp).Should().BeTrue();
            accessProp.GetString()!.Should().NotBeNullOrWhiteSpace();
            json.TryGetProperty("refreshToken", out var _).Should().BeFalse();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_RefreshV2_Should_RotateCookieAndReturnAccess_WhenCookieValid] Response: {body}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_RefreshV2_Should_Return401_WhenReusingOldRotatedCookie()
    {
        // Arrange
        var loginDto = new { Email = "v2_refresh@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt-v2", loginDto);
        loginResp.EnsureSuccessStatusCode();
        loginResp.Headers.TryGetValues("Set-Cookie", out var initialCookies).Should().BeTrue();
        var oldRefresh = ExtractCookieValue(initialCookies!, "refresh_token");
        var firstRefreshRequest = new HttpRequestMessage(HttpMethod.Post, "/refresh-jwt-v2");
        firstRefreshRequest.Headers.Add("Cookie", $"refresh_token={oldRefresh}");
        var firstRefresh = await Client.SendAsync(firstRefreshRequest);
        firstRefresh.EnsureSuccessStatusCode();
        firstRefresh.Headers.TryGetValues("Set-Cookie", out var newCookies).Should().BeTrue();
        var newRefresh = ExtractCookieValue(newCookies!, "refresh_token");
        newRefresh.Should().NotBe(oldRefresh);
        var manualRequest = new HttpRequestMessage(HttpMethod.Post, "/refresh-jwt-v2");
        manualRequest.Headers.Add("Cookie", $"refresh_token={oldRefresh}");

        // Act
        var reuseResp = await Client.SendAsync(manualRequest);
        var body = await reuseResp.Content.ReadAsStringAsync();

        // Assert
        try
        {
            reuseResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_RefreshV2_Should_Return401_WhenReusingOldRotatedCookie] Response: {body}");
            throw;
        }
    }
}