namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class LoginTests : BaseEndpointTest
{
    public LoginTests(IntegrationApiFactory factory) : base(factory)
    {
        // Arrange: подготовка тестовых пользователей
        SeedUserAsync("unconfirmed@example.com", "password123", false).GetAwaiter().GetResult();
        SeedUserAsync("confirmed@example.com", "password123", true).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Endpoint_Login_Should_ReturnValidationError_WhenEmailEmpty()
    {
        // Arrange
        var dto = new { Email = "", Password = "password123" };

        // Act
        var response = await Client.PostAsJsonAsync("/login-jwt", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

            var json = JsonDocument.Parse(jsonString).RootElement;

            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");

            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString()!.Should().NotBeNullOrWhiteSpace();

            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.ValueKind.Should().Be(JsonValueKind.Array);
            errors.GetArrayLength().Should().BeGreaterThan(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Login_Should_ReturnValidationError_WhenEmailEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Login_Should_ReturnValidationError_WhenPasswordEmpty()
    {
        // Arrange
        var dto = new { Email = "user@example.com", Password = "" };

        // Act
        var response = await Client.PostAsJsonAsync("/login-jwt", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

            var json = JsonDocument.Parse(jsonString).RootElement;

            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");

            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString()!.Should().NotBeNullOrWhiteSpace();

            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.ValueKind.Should().Be(JsonValueKind.Array);
            errors.GetArrayLength().Should().BeGreaterThan(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Login_Should_ReturnValidationError_WhenPasswordEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Login_Should_ReturnValidationError_WhenEmailFormatInvalid()
    {
        // Arrange
        var dto = new { Email = "invalid-email", Password = "password123" };

        // Act
        var response = await Client.PostAsJsonAsync("/login-jwt", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

            var json = JsonDocument.Parse(jsonString).RootElement;

            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");

            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString()!.Should().NotBeNullOrWhiteSpace();

            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.ValueKind.Should().Be(JsonValueKind.Array);
            errors.GetArrayLength().Should().BeGreaterThan(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Login_Should_ReturnValidationError_WhenEmailFormatInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Login_Should_ReturnBadRequest_WhenCredentialsInvalid()
    {
        // Arrange
        var dto = new { Email = "confirmed@example.com", Password = "wrongpassword" };

        // Act
        var response = await Client.PostAsJsonAsync("/login-jwt", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var json = JsonDocument.Parse(jsonString).RootElement;

            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("InvalidCredentials");

            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString()!.Should().NotBeNullOrWhiteSpace();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Login_Should_ReturnBadRequest_WhenCredentialsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Login_Should_ReturnEmailConfirmedFalse_WhenEmailNotConfirmed()
    {
        // Arrange
        var dto = new { Email = "unconfirmed@example.com", Password = "password123" };

        // Act
        var response = await Client.PostAsJsonAsync("/login-jwt", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = JsonDocument.Parse(jsonString).RootElement;

            json.TryGetProperty("emailConfirmed", out var emailConfirmed).Should().BeTrue();
            emailConfirmed.GetBoolean().Should().BeFalse();

            json.TryGetProperty("accessToken", out var _).Should().BeFalse();
            json.TryGetProperty("refreshToken", out var _).Should().BeFalse();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Login_Should_ReturnEmailConfirmedFalse_WhenEmailNotConfirmed] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Login_Should_ReturnTokens_WhenEmailConfirmed()
    {
        // Arrange
        var dto = new { Email = "confirmed@example.com", Password = "password123" };

        // Act
        var response = await Client.PostAsJsonAsync("/login-jwt", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = JsonDocument.Parse(jsonString).RootElement;

            json.TryGetProperty("emailConfirmed", out var emailConfirmed).Should().BeTrue();
            emailConfirmed.GetBoolean().Should().BeTrue();

            json.TryGetProperty("accessToken", out var accessToken).Should().BeTrue();
            accessToken.GetString()!.Should().NotBeNullOrWhiteSpace();

            json.TryGetProperty("refreshToken", out var refreshToken).Should().BeTrue();
            refreshToken.GetString()!.Should().NotBeNullOrWhiteSpace();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Login_Should_ReturnTokens_WhenEmailConfirmed] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Login_Should_ReturnSuccess_WhenEmailCaseInsensitive()
    {
        // Arrange
        var dto = new { Email = "CONFIRMED@EXAMPLE.COM", Password = "password123" };

        // Act
        var response = await Client.PostAsJsonAsync("/login-jwt", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("emailConfirmed", out var emailConfirmed).Should().BeTrue();
            emailConfirmed.GetBoolean().Should().BeTrue();
            json.TryGetProperty("accessToken", out var accessToken).Should().BeTrue();
            accessToken.GetString()!.Should().NotBeNullOrWhiteSpace();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Login_Should_ReturnSuccess_WhenEmailCaseInsensitive] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Login_Should_ReturnValidationError_WhenFieldsWhitespace()
    {
        // Arrange
        var dto = new { Email = "   ", Password = "   " };

        // Act
        var response = await Client.PostAsJsonAsync("/login-jwt", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Login_Should_ReturnValidationError_WhenFieldsWhitespace] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Login_Should_ReturnValidationError_WhenInputsTooLong()
    {
        // Arrange
        var longEmail = new string('a', 500) + "@example.com";
        var longPass = new string('p', 2000);
        var dto = new { Email = longEmail, Password = longPass };

        // Act
        var response = await Client.PostAsJsonAsync("/login-jwt", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            var status = (int)response.StatusCode;
            (status == 422 || status == 400).Should().BeTrue();
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            var codeStr = code.GetString();
            (codeStr == "ValidationError" || codeStr == "InvalidCredentials").Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Login_Should_ReturnValidationError_WhenInputsTooLong] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Login_Should_ReturnTokenWithClaims_WhenSuccess()
    {
        // Arrange
        var dto = new { Email = "confirmed@example.com", Password = "password123" };

        // Act
        var response = await Client.PostAsJsonAsync("/login-jwt", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("accessToken", out var accessToken).Should().BeTrue();
            var token = accessToken.GetString()!;
            token.Should().NotBeNullOrWhiteSpace();

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "confirmed@example.com");
            jwt.Claims.Should().Contain(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Login_Should_ReturnTokenWithClaims_WhenSuccess] Response: {jsonString}");
            throw;
        }
    }
}
