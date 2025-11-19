namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class RegistrationTests : BaseEndpointTest
{
    public RegistrationTests(IntegrationApiFactory factory) : base(factory)
    {
    }

    private HttpClient CreateClientWithDisabledRateLimiter()
    {
        var overrides = new Dictionary<string, string>
        {
            ["RateLimiter:EmailSms:MinIntervalSeconds"] = "0",
            ["RateLimiter:EmailSms:WindowMinutes"] = "1",
            ["RateLimiter:EmailSms:MaxRequests"] = "10000"
        };

        return Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) => conf.AddInMemoryCollection(overrides));
        }).CreateClient();
    }

    [Fact]
    public async Task RegisterMock_Should_ReturnCallbackUrl()
    {
        // Arrange
        var unique = Guid.NewGuid().ToString()[..8];
        var dto = new { Username = $"new_user_{unique}", Email = $"{unique}@example.com", Password = "P@ssw0rd!" };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync("/registerWithUsername-mock", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("callbackUrl", out var callback).Should().BeTrue();
            callback.GetString().Should().NotBeNullOrWhiteSpace();
        }
        catch (Exception)
        {
            Console.WriteLine($"[RegisterMock_Should_ReturnCallbackUrl] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("shortpwd", "Пароль должен содержать хотя бы одну цифру")]
    [InlineData("abcdef", "Пароль должен содержать хотя бы одну цифру")]
    [InlineData("ABC123", "Пароль должен содержать хотя бы одну строчную букву")]
    public async Task RegisterMock_PasswordPolicy_Violations_ReturnIdentityErrors(string password, string expectedMessagePart)
    {
        // Arrange
        var unique = Guid.NewGuid().ToString()[..8];
        var dto = new { Username = $"user_{unique}", Email = $"{unique}@example.com", Password = password };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync("/registerWithUsername-mock", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            var found = errors.EnumerateArray().Any(e => e.GetProperty("message").GetString()?.Contains(expectedMessagePart, StringComparison.OrdinalIgnoreCase) == true);
            found.Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[RegisterMock_PasswordPolicy_Violations_ReturnIdentityErrors] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task RegisterMock_DuplicateUsernameOrEmail_ReturnsBadRequestWithDuplicateMessage()
    {
        // Arrange
        var unique = Guid.NewGuid().ToString()[..8];
        var dto = new { Username = $"dup_{unique}", Email = $"dup_{unique}@example.com", Password = "P@ssw0rd!" };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var first = await client.PostAsJsonAsync("/registerWithUsername-mock", dto);
        first.EnsureSuccessStatusCode();

        var second = await client.PostAsJsonAsync("/registerWithUsername-mock", dto);
        var jsonString = await second.Content.ReadAsStringAsync();

        // Assert
        try
        {
            second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            var any = errors.EnumerateArray().Any(e =>
            {
                var msg = e.GetProperty("message").GetString() ?? string.Empty;
                return msg.Contains("уже используется", StringComparison.OrdinalIgnoreCase) || msg.Contains("already");
            });
            any.Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[RegisterMock_DuplicateUsernameOrEmail_ReturnsBadRequestWithDuplicateMessage] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("", "user@example.com", "P@ssw0rd!", "Username")]
    [InlineData("user", "", "P@ssw0rd!", "Email")]
    [InlineData("user", "invalid-email", "P@ssw0rd!", "Email")]
    [InlineData("user", "user@example.com", "", "Password")]
    public async Task RegisterMock_InvalidModel_ReturnsValidationError(string username, string email, string password, string expectedField)
    {
        // Arrange
        var dto = new { Username = username, Email = email, Password = password };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync("/registerWithUsername-mock", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("ValidationError");

            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.ValueKind.Should().Be(JsonValueKind.Array);
            var keywords = expectedField switch
            {
                "Username" => new[] { "Логин", "username", "Username" },
                "Email" => new[] { "Email", "email", "Некорректный формат email", "Email обязателен" },
                "Password" => new[] { "Пароль", "password", "Password" },
                _ => new[] { expectedField }
            };

            var found = errors.EnumerateArray().Any(e =>
            {
                if (!e.TryGetProperty("message", out var msg)) return false;
                var m = msg.GetString();
                if (string.IsNullOrEmpty(m)) return false;
                return keywords.Any(k => m.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
            });

            found.Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[RegisterMock_InvalidModel_ReturnsValidationError] Response: {jsonString}");
            throw;
        }
    }
}
