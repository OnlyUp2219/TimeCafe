namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class RefreshTokenTests : BaseEndpointTest
{
    public RefreshTokenTests(IntegrationApiFactory factory) : base(factory)
    {
        SeedUserAsync("refresh_user_valid@example.com", "P@ssw0rd!", true).GetAwaiter().GetResult();
    }

    private const string LoginEndpoint = "/login-jwt-v2";
    private const string RefreshEndpoint = "/refresh-jwt-v2";

    private static string ExtractCookieValue(IEnumerable<string> setCookies, string name)
    {
        var raw = setCookies.First(c => c.StartsWith(name + "=", StringComparison.OrdinalIgnoreCase));
        var firstPart = raw.Split(';', 2)[0];
        return firstPart.Substring(name.Length + 1);
    }

    private static HttpRequestMessage CreateRefreshRequest(string refreshToken)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, RefreshEndpoint)
        {
            Content = new StringContent(string.Empty)
        };
        req.Headers.Add("Cookie", $"refresh_token={refreshToken}");
        return req;
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_ReturnUnauthorized_WhenTokenInvalid()
    {
        // Arrange
        var request = CreateRefreshRequest("invalid-token-value");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_ReturnUnauthorized_WhenTokenEmpty()
    {
        // Arrange
        var request = CreateRefreshRequest(string.Empty);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_ReturnNewTokens_WhenTokenValid()
    {
        // Arrange
        var loginDto = new { Email = "refresh_user_valid@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync(LoginEndpoint, loginDto);
        loginResp.EnsureSuccessStatusCode();

        loginResp.Headers.TryGetValues("Set-Cookie", out var initialCookies).Should().BeTrue();
        var oldRefresh = ExtractCookieValue(initialCookies!, "refresh_token");

        // Act
        var response = await Client.SendAsync(CreateRefreshRequest(oldRefresh));
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Headers.TryGetValues("Set-Cookie", out var newCookies).Should().BeTrue();
        var newRefresh = ExtractCookieValue(newCookies!, "refresh_token");
        newRefresh.Should().NotBe(oldRefresh);

        var newTokens = JsonDocument.Parse(jsonString).RootElement;
        newTokens.TryGetProperty("accessToken", out var access).Should().BeTrue();
        access.GetString()!.Should().NotBeNullOrWhiteSpace();
        newTokens.TryGetProperty("refreshToken", out var _).Should().BeFalse();
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_InvalidateOldToken_WhenRefreshUsed()
    {
        // Arrange
        var loginDto = new { Email = "refresh_user_valid@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync(LoginEndpoint, loginDto);
        loginResp.EnsureSuccessStatusCode();

        loginResp.Headers.TryGetValues("Set-Cookie", out var initialCookies).Should().BeTrue();
        var oldRefresh = ExtractCookieValue(initialCookies!, "refresh_token");

        // Act
        var first = await Client.SendAsync(CreateRefreshRequest(oldRefresh));
        first.EnsureSuccessStatusCode();

        var second = await Client.SendAsync(CreateRefreshRequest(oldRefresh));

        // Assert
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_RevokeTokenFamily_WhenRevokedTokenReused()
    {
        // Arrange: Create token chain (token1 -> token2 -> token3)
        var loginDto = new { Email = "refresh_user_valid@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync(LoginEndpoint, loginDto);
        loginResp.EnsureSuccessStatusCode();

        loginResp.Headers.TryGetValues("Set-Cookie", out var cookies1).Should().BeTrue();
        var token1 = ExtractCookieValue(cookies1!, "refresh_token");

        var refresh1 = await Client.SendAsync(CreateRefreshRequest(token1));
        refresh1.EnsureSuccessStatusCode();
        refresh1.Headers.TryGetValues("Set-Cookie", out var cookies2).Should().BeTrue();
        var token2 = ExtractCookieValue(cookies2!, "refresh_token");

        var refresh2 = await Client.SendAsync(CreateRefreshRequest(token2));
        refresh2.EnsureSuccessStatusCode();
        refresh2.Headers.TryGetValues("Set-Cookie", out var cookies3).Should().BeTrue();
        var token3 = ExtractCookieValue(cookies3!, "refresh_token");

        // Act: Reuse revoked token2 (token theft detected)
        var reuseRevoked = await Client.SendAsync(CreateRefreshRequest(token2));

        // Assert: Entire family should be revoked
        reuseRevoked.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var useToken3 = await Client.SendAsync(CreateRefreshRequest(token3));
        useToken3.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_LinkTokens_WhenRotationHappens()
    {
        // Arrange
        var loginDto = new { Email = "refresh_user_valid@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync(LoginEndpoint, loginDto);
        loginResp.EnsureSuccessStatusCode();

        loginResp.Headers.TryGetValues("Set-Cookie", out var cookies1).Should().BeTrue();
        var oldToken = ExtractCookieValue(cookies1!, "refresh_token");

        // Act
        var refreshResp = await Client.SendAsync(CreateRefreshRequest(oldToken));
        refreshResp.EnsureSuccessStatusCode();

        refreshResp.Headers.TryGetValues("Set-Cookie", out var cookies2).Should().BeTrue();
        var newToken = ExtractCookieValue(cookies2!, "refresh_token");

        // Assert
        newToken.Should().NotBe(oldToken);

        var useNew = await Client.SendAsync(CreateRefreshRequest(newToken));
        useNew.StatusCode.Should().Be(HttpStatusCode.OK);

        var reuseOld = await Client.SendAsync(CreateRefreshRequest(oldToken));
        reuseOld.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
