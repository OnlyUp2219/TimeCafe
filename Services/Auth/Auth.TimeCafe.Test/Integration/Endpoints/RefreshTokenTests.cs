namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class RefreshTokenTests : BaseEndpointTest
{
    public RefreshTokenTests(IntegrationApiFactory factory) : base(factory)
    {
        SeedUserAsync("refresh_user_valid@example.com", "P@ssw0rd!", true).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_ReturnUnauthorized_WhenTokenInvalid()
    {
        // Arrange
        var dto = new { RefreshToken = "invalid-token-value" };

        // Act
        var response = await Client.PostAsJsonAsync("/refresh-token-jwt", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_ReturnUnauthorized_WhenTokenEmpty()
    {
        // Arrange
        var dto = new { RefreshToken = "" };

        // Act
        var response = await Client.PostAsJsonAsync("/refresh-token-jwt", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_ReturnNewTokens_WhenTokenValid()
    {
        // Arrange
        var loginDto = new { Email = "refresh_user_valid@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt", loginDto);
        loginResp.EnsureSuccessStatusCode();

        var json = await loginResp.Content.ReadAsStringAsync();
        var tokens = JsonDocument.Parse(json).RootElement;
        var refresh = tokens.GetProperty("refreshToken").GetString()!;

        // Act
        var response = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = refresh });
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var newTokens = JsonDocument.Parse(jsonString).RootElement;
        newTokens.TryGetProperty("accessToken", out var access).Should().BeTrue();
        newTokens.TryGetProperty("refreshToken", out var newRefresh).Should().BeTrue();

        access.GetString()!.Should().NotBeNullOrWhiteSpace();
        newRefresh.GetString()!.Should().NotBeNullOrWhiteSpace();
        newRefresh.GetString()!.Should().NotBe(refresh);
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_InvalidateOldToken_WhenRefreshUsed()
    {
        // Arrange
        var loginDto = new { Email = "refresh_user_valid@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt", loginDto);
        loginResp.EnsureSuccessStatusCode();

        var json = await loginResp.Content.ReadAsStringAsync();
        var tokens = JsonDocument.Parse(json).RootElement;
        var refresh = tokens.GetProperty("refreshToken").GetString()!;

        // Act
        var first = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = refresh });
        first.EnsureSuccessStatusCode();

        var second = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = refresh });

        // Assert
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_RevokeTokenFamily_WhenRevokedTokenReused()
    {
        // Arrange: Create token chain (token1 -> token2 -> token3)
        var loginDto = new { Email = "refresh_user_valid@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt", loginDto);
        loginResp.EnsureSuccessStatusCode();

        var json1 = await loginResp.Content.ReadAsStringAsync();
        var token1 = JsonDocument.Parse(json1).RootElement.GetProperty("refreshToken").GetString()!;

        var refresh1 = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = token1 });
        refresh1.EnsureSuccessStatusCode();
        var json2 = await refresh1.Content.ReadAsStringAsync();
        var token2 = JsonDocument.Parse(json2).RootElement.GetProperty("refreshToken").GetString()!;

        var refresh2 = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = token2 });
        refresh2.EnsureSuccessStatusCode();
        var json3 = await refresh2.Content.ReadAsStringAsync();
        var token3 = JsonDocument.Parse(json3).RootElement.GetProperty("refreshToken").GetString()!;

        // Act: Reuse revoked token2 (token theft detected)
        var reuseRevoked = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = token2 });

        // Assert: Entire family should be revoked
        reuseRevoked.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var useToken3 = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = token3 });
        useToken3.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_LinkTokens_WhenRotationHappens()
    {
        // Arrange
        var loginDto = new { Email = "refresh_user_valid@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt", loginDto);
        loginResp.EnsureSuccessStatusCode();

        var json = await loginResp.Content.ReadAsStringAsync();
        var oldToken = JsonDocument.Parse(json).RootElement.GetProperty("refreshToken").GetString()!;

        // Act
        var refreshResp = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = oldToken });
        refreshResp.EnsureSuccessStatusCode();

        var newJson = await refreshResp.Content.ReadAsStringAsync();
        var newToken = JsonDocument.Parse(newJson).RootElement.GetProperty("refreshToken").GetString()!;

        // Assert
        newToken.Should().NotBe(oldToken);

        var useNew = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = newToken });
        useNew.StatusCode.Should().Be(HttpStatusCode.OK);

        var reuseOld = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = oldToken });
        reuseOld.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
