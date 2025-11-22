namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class LogoutTests : BaseEndpointTest
{
    public LogoutTests(IntegrationApiFactory factory) : base(factory)
    {
        SeedUser("confirmed@example.com", "password123", true);
    }
    [Fact]
    public async Task Endpoint_Logout_Should_ReturnValidationError_WhenTokenEmpty()
    {
        // Arrange
        var dto = new { RefreshToken = "" };

        // Act
        var response = await Client.PostAsJsonAsync("/logout", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be((HttpStatusCode)422);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");
            json.TryGetProperty("status", out var status).Should().BeTrue();
            status.GetInt32().Should().Be(422);
            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors[0].TryGetProperty("message", out var errMsg).Should().BeTrue();
            errMsg.GetString()!.Should().Contain("Base64");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Logout_Should_ReturnValidationError_WhenTokenEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Logout_Should_ReturnSuccess_WhenTokenValid()
    {
        // Arrange
        var loginDto = new { Email = "confirmed@example.com", Password = "password123" };

        // Act
        var loginResp = await Client.PostAsJsonAsync("/login-jwt", loginDto);
        var loginJsonString = await loginResp.Content.ReadAsStringAsync();

        // Assert
        try
        {
            loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var loginJson = JsonDocument.Parse(loginJsonString).RootElement;
            loginJson.TryGetProperty("refreshToken", out var refreshToken).Should().BeTrue();
            var logoutDto = new { RefreshToken = refreshToken.GetString() };
            var response = await Client.PostAsJsonAsync("/logout", logoutDto);
            var jsonString = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("revoked", out var revoked).Should().BeTrue();
            revoked.GetBoolean().Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Logout_Should_ReturnSuccess_WhenTokenValid] Login Response: {loginJsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Logout_Should_ReturnNotRevoked_WhenTokenAlreadyRevoked()
    {
        // Arrange
        var loginDto = new { Email = "confirmed@example.com", Password = "password123" };

        // Act
        var loginResp = await Client.PostAsJsonAsync("/login-jwt", loginDto);
        var loginJsonString = await loginResp.Content.ReadAsStringAsync();

        // Assert
        try
        {
            loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var loginJson = JsonDocument.Parse(loginJsonString).RootElement;
            loginJson.TryGetProperty("refreshToken", out var refreshToken).Should().BeTrue();
            var token = refreshToken.GetString();
            var logoutDto = new { RefreshToken = token };
            var firstLogout = await Client.PostAsJsonAsync("/logout", logoutDto);
            var firstLogoutString = await firstLogout.Content.ReadAsStringAsync();
            firstLogout.StatusCode.Should().Be(HttpStatusCode.OK);
            var secondLogout = await Client.PostAsJsonAsync("/logout", logoutDto);
            var secondLogoutString = await secondLogout.Content.ReadAsStringAsync();

            secondLogout.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(secondLogoutString).RootElement;
            json.TryGetProperty("revoked", out var revoked).Should().BeTrue();
            revoked.GetBoolean().Should().BeFalse();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Logout_Should_ReturnNotRevoked_WhenTokenAlreadyRevoked] Login Response: {loginJsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Logout_Should_ReturnValidationError_WhenTokenNonexistent()
    {
        // Arrange
        var logoutDto = new { RefreshToken = "nonexistent-token-123" };

        // Act
        var response = await Client.PostAsJsonAsync("/logout", logoutDto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be((HttpStatusCode)422);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");
            json.TryGetProperty("status", out var status).Should().BeTrue();
            status.GetInt32().Should().Be(422);
            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors[0].TryGetProperty("message", out var errMsg).Should().BeTrue();
            errMsg.GetString()!.Should().Contain("Base64");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Logout_Should_ReturnValidationError_WhenTokenNonexistent] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Logout_Should_ReturnValidationError_WhenTokenMalformed()
    {
        // Arrange
        var logoutDto = new { RefreshToken = "!!!@@@###" };

        // Act
        var response = await Client.PostAsJsonAsync("/logout", logoutDto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be((HttpStatusCode)422);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");
            json.TryGetProperty("status", out var status).Should().BeTrue();
            status.GetInt32().Should().Be(422);
            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors[0].TryGetProperty("message", out var errMsg).Should().BeTrue();
            errMsg.GetString()!.Should().Contain("Base64");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Logout_Should_ReturnValidationError_WhenTokenMalformed] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Logout_Should_ReturnValidationError_WhenTokenNullOrMissing()
    {
        // Arrange
        var logoutDto = new { };

        // Act
        var response = await Client.PostAsJsonAsync("/logout", logoutDto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be((HttpStatusCode)422);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");
            json.TryGetProperty("status", out var status).Should().BeTrue();
            status.GetInt32().Should().Be(422);
            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors[0].TryGetProperty("message", out var errMsg).Should().BeTrue();
            errMsg.GetString()!.Should().Contain("Base64");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Logout_Should_ReturnValidationError_WhenTokenNullOrMissing] Response: {jsonString}");
            throw;
        }
    }


    [Fact]
    public async Task Endpoint_Logout_Should_ReturnNotRevoked_WhenTokenBase64ButNonexistent()
    {
        // Arrange
        var raw = "nonexistent-token-456";
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        var logoutDto = new { RefreshToken = base64 };

        // Act
        var response = await Client.PostAsJsonAsync("/logout", logoutDto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("revoked", out var revoked).Should().BeTrue();
            revoked.GetBoolean().Should().BeFalse();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Logout_Should_ReturnNotRevoked_WhenTokenBase64ButNonexistent] Response: {jsonString}");
            throw;
        }
    }
}
