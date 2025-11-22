namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class EmailConfirmationTests : BaseEndpointTest
{
    private const string Endpoint = "/email/confirm";

    public EmailConfirmationTests(IntegrationApiFactory factory) : base(factory)
    {
        SeedUser("unconfirmed@example.com", "P@ssw0rd!", false);
        SeedUser("confirmed@example.com", "P@ssw0rd!", true);
        SeedUser("another_user@example.com", "P@ssw0rd!", false);
    }

    [Fact]
    public async Task Endpoint_ConfirmEmail_Should_ReturnUserNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var dto = new { UserId = "nonexistent-user-id", Token = "dummy-token" };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be((HttpStatusCode)401);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("UserNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ConfirmEmail_Should_ReturnUserNotFound_WhenUserDoesNotExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ConfirmEmail_Should_ReturnEmailAlreadyConfirmed_WhenAlreadyConfirmed()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByEmailAsync("confirmed@example.com");
        var dto = new { UserId = user!.Id, Token = "dummy-token" };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("EmailAlreadyConfirmed");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ConfirmEmail_Should_ReturnEmailAlreadyConfirmed_WhenAlreadyConfirmed] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ConfirmEmail_Should_ReturnInvalidTokenFormat_WhenTokenNotBase64Url()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByEmailAsync("unconfirmed@example.com");
        var dto = new { UserId = user!.Id, Token = "!!!@@@###" };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("InvalidTokenFormat");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ConfirmEmail_Should_ReturnInvalidTokenFormat_WhenTokenNotBase64Url] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ConfirmEmail_Should_ReturnInvalidToken_WhenTokenIncorrect()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByEmailAsync("unconfirmed@example.com");
        var invalidToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("invalid-token-value"));
        var dto = new { UserId = user!.Id, Token = invalidToken };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("InvalidToken");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ConfirmEmail_Should_ReturnInvalidToken_WhenTokenIncorrect] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ConfirmEmail_Should_ReturnSuccess_WhenValidToken()
    {
        // Arrange
        var email = "unconfirmed@example.com";
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByEmailAsync(email);
        var token = await GenerateConfirmationTokenAsync(email);
        var dto = new { UserId = user!.Id, Token = token };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString()!.Should().Be("Email подтвержден");
            (await IsEmailConfirmedAsync(email)).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ConfirmEmail_Should_ReturnSuccess_WhenValidToken] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("", "dummy-token", "Пользователь не найден")]
    public async Task Endpoint_ConfirmEmail_Should_ReturnValidationError_WhenInvalidCommand(string userId, string token, string expectedMessagePart)
    {
        // Arrange
        var dto = new { UserId = userId, Token = token };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be((HttpStatusCode)422);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");
            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.EnumerateArray().Any(e => e.TryGetProperty("message", out var msg) && msg.GetString()?.Contains(expectedMessagePart) == true).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ConfirmEmail_Should_ReturnValidationError_WhenInvalidCommand] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ConfirmEmail_Should_ReturnValidationError_WhenEmptyRequestBody()
    {
        // Arrange
        var dto = new { };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be((HttpStatusCode)422);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");
            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.GetArrayLength().Should().BeGreaterThan(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ConfirmEmail_Should_ReturnValidationError_WhenEmptyRequestBody] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ConfirmEmail_Should_ReturnInvalidToken_WhenMalformedToken()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByEmailAsync("unconfirmed@example.com");
        var malformedToken = "invalid_base64_without_padding";
        var dto = new { UserId = user!.Id, Token = malformedToken };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("InvalidToken");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ConfirmEmail_Should_ReturnInvalidToken_WhenMalformedToken] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ConfirmEmail_Should_ReturnInvalidToken_WhenTokenFromAnotherUser()
    {
        // Arrange
        var anotherEmail = "another_user@example.com";
        var token = await GenerateConfirmationTokenAsync(anotherEmail);
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByEmailAsync("unconfirmed@example.com");
        var dto = new { UserId = user!.Id, Token = token };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("InvalidToken");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ConfirmEmail_Should_ReturnInvalidToken_WhenTokenFromAnotherUser] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ConfirmEmail_Should_WorkWithRateLimiterOverrides()
    {
        // Arrange
        var email = "unconfirmed@example.com";
        var token = await GenerateConfirmationTokenAsync(email);
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByEmailAsync(email);
        var dto = new { UserId = user!.Id, Token = token };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ConfirmEmail_Should_WorkWithRateLimiterOverrides] Response: {jsonString}");
            throw;
        }
    }
}