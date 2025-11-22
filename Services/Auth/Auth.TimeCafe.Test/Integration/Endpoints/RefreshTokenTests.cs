namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class RefreshTokenTests : BaseEndpointTest
{
    public RefreshTokenTests(IntegrationApiFactory factory) : base(factory)
    {
        SeedUser("refresh_user_valid@example.com", "P@ssw0rd!", true);
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
        var refresh = tokens.GetProperty("refreshToken").GetString();

        var dto = new { RefreshToken = refresh };

        // Act
        var first = await Client.PostAsJsonAsync("/refresh-token-jwt", dto);
        first.EnsureSuccessStatusCode();

        var second = await Client.PostAsJsonAsync("/refresh-token-jwt", dto);

        // Assert
        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
    public async Task Endpoint_RefreshToken_Should_RevokeTokenFamily_WhenRevokedTokenReused()
    {
        // Arrange: Login and create token chain (token1 -> token2 -> token3)
        var loginDto = new { Email = "refresh_user_valid@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt", loginDto);
        loginResp.EnsureSuccessStatusCode();
        
        var json1 = await loginResp.Content.ReadAsStringAsync();
        var tokens1 = JsonDocument.Parse(json1).RootElement;
        var token1 = tokens1.GetProperty("refreshToken").GetString()!;

        // Refresh to get token2
        var refresh1 = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = token1 });
        refresh1.EnsureSuccessStatusCode();
        var json2 = await refresh1.Content.ReadAsStringAsync();
        var tokens2 = JsonDocument.Parse(json2).RootElement;
        var token2 = tokens2.GetProperty("refreshToken").GetString()!;

        // Refresh to get token3
        var refresh2 = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = token2 });
        refresh2.EnsureSuccessStatusCode();
        var json3 = await refresh2.Content.ReadAsStringAsync();
        var tokens3 = JsonDocument.Parse(json3).RootElement;
        var token3 = tokens3.GetProperty("refreshToken").GetString()!;

        // Act: Try to reuse revoked token2 (suspicious activity - possible token theft)
        var reuseRevoked = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = token2 });

        // Assert: Should reject and revoke entire family
        reuseRevoked.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "reused revoked token should be rejected");

        // Verify token3 (latest in chain) is also revoked
        var useToken3 = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = token3 });
        useToken3.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "entire token family should be revoked");
    }

    [Fact]
    public async Task Endpoint_RefreshToken_Should_LinkTokens_WhenRotationHappens()
    {
        // Arrange: Login to get initial token
        var loginDto = new { Email = "refresh_user_valid@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt", loginDto);
        loginResp.EnsureSuccessStatusCode();
        
        var json = await loginResp.Content.ReadAsStringAsync();
        var tokens = JsonDocument.Parse(json).RootElement;
        var oldToken = tokens.GetProperty("refreshToken").GetString()!;

        // Act: Refresh to trigger rotation
        var refreshResp = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = oldToken });
        refreshResp.EnsureSuccessStatusCode();
        
        var newJson = await refreshResp.Content.ReadAsStringAsync();
        var newTokens = JsonDocument.Parse(newJson).RootElement;
        var newToken = newTokens.GetProperty("refreshToken").GetString()!;

        // Assert: Tokens should be different (rotation happened)
        newToken.Should().NotBe(oldToken, "token rotation should generate new token");

        // New token should work (before any suspicious activity)
        var useNew = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = newToken });
        useNew.StatusCode.Should().Be(HttpStatusCode.OK, "new token should be valid");

        // Old token should be revoked (and will trigger family revocation)
        var reuseOld = await Client.PostAsJsonAsync("/refresh-token-jwt", new { RefreshToken = oldToken });
        reuseOld.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "old token should be revoked after rotation");
    }
}
