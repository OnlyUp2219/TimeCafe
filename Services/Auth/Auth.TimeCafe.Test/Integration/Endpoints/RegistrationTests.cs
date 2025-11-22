namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class RegistrationTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_Register_Should_ReturnCallbackUrl_WhenValidRequest()
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
            callback.GetString()!.Should().NotBeNullOrWhiteSpace();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Register_Should_ReturnCallbackUrl_WhenValidRequest] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("shortpwd", "Пароль должен содержать хотя бы одну цифру")]
    [InlineData("abcdef", "Пароль должен содержать хотя бы одну цифру")]
    [InlineData("ABC123", "Пароль должен содержать хотя бы одну строчную букву")]
    public async Task Endpoint_Register_Should_ReturnBadRequest_WhenPasswordPolicyViolated(string password, string expectedMessagePart)
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
            Console.WriteLine($"[Endpoint_Register_Should_ReturnBadRequest_WhenPasswordPolicyViolated] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Register_Should_ReturnBadRequest_WhenUsernameOrEmailDuplicate()
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
            Console.WriteLine($"[Endpoint_Register_Should_ReturnBadRequest_WhenUsernameOrEmailDuplicate] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("", "user@example.com", "P@ssw0rd!", "Username")]
    [InlineData("user", "", "P@ssw0rd!", "Email")]
    [InlineData("user", "invalid-email", "P@ssw0rd!", "Email")]
    [InlineData("user", "user@example.com", "", "Password")]
    public async Task Endpoint_Register_Should_ReturnValidationError_WhenModelInvalid(string username, string email, string password, string expectedField)
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
            code.GetString()!.Should().Be("ValidationError");

            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.ValueKind.Should().Be(JsonValueKind.Array);
            string[] value = ["Email", "email", "Некорректный формат email", "Email обязателен"];
            var keywords = expectedField switch
            {
                "Username" => ["Логин", "username", "Username"],
                "Email" => value,
                "Password" => ["Пароль", "password", "Password"],
                _ => [expectedField]
            };

            var found = errors.EnumerateArray().Any(e =>
            {
                if (!e.TryGetProperty("message", out var msg)) return false;
                var m = msg.GetString();
                if (string.IsNullOrEmpty(m)) return false;
                return keywords.Any(k => m.Contains(k, StringComparison.OrdinalIgnoreCase));
            });

            found.Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Register_Should_ReturnValidationError_WhenModelInvalid] Response: {jsonString}");
            throw;
        }
    }
}
