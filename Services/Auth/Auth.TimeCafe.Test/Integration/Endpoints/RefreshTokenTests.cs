namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class RefreshTokenTests : BaseEndpointTest
{
    public RefreshTokenTests(IntegrationApiFactory factory) : base(factory)
    {
        SeedUser("refresh_user_valid@example.com", "P@ssw0rd!", true);
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_Should_ReturnUnauthorized()
    {
        // Arrange
        var dto = new { RefreshToken = "invalid-token-value" };

        // Act
        var response = await Client.PostAsJsonAsync("/refresh-token-jwt", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        catch (Exception)
        {
            Console.WriteLine($"[RefreshToken_InvalidToken_Should_ReturnUnauthorized] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task RefreshToken_ValidToken_Should_ReturnNewTokens()
    {
        // Arrange: login to obtain tokens
        var loginDto = new { Email = "refresh_user_valid@example.com", Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt", loginDto);
        loginResp.EnsureSuccessStatusCode();
        var json = await loginResp.Content.ReadAsStringAsync();
        var tokens = JsonDocument.Parse(json).RootElement;
        var refresh = tokens.GetProperty("refreshToken").GetString();

        var dto = new { RefreshToken = refresh };

        // Act
        var response = await Client.PostAsJsonAsync("/refresh-token-jwt", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var newTokens = JsonDocument.Parse(jsonString).RootElement;
            newTokens.TryGetProperty("accessToken", out var access).Should().BeTrue();
            newTokens.TryGetProperty("refreshToken", out var newRefresh).Should().BeTrue();

            access.GetString().Should().NotBeNullOrWhiteSpace();
            newRefresh.GetString().Should().NotBeNullOrWhiteSpace();
            newRefresh.GetString().Should().NotBe(refresh);
        }
        catch (Exception)
        {
            Console.WriteLine($"[RefreshToken_ValidToken_Should_ReturnNewTokens] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task RefreshToken_ConcurrentUse_OldTokenBecomesInvalid()
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
}
