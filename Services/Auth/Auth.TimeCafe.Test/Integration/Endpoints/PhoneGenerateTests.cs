namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class PhoneGenerateTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    private const string Endpoint = "/twilio/generateSMS-mock";

    [Fact]
    public async Task Endpoint_GenerateSmsMock_Should_ReturnUnauthorized_WhenNoToken()
    {
        // Arrange
        var dto = new { PhoneNumber = "+79123456789" };
        using var client = CreateClientWithDisabledRateLimiter();

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, dto);

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GenerateSmsMock_Should_ReturnUnauthorized_WhenNoToken] StatusCode: {response.StatusCode}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GenerateSmsMock_Should_ReturnUserNotFound_WhenUserNotInDb()
    {
        // Arrange — токен валидный, но пользователя нет (удаляем после логина)
        var (userId, accessToken) = await CreateAuthenticatedUserAsync();

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        await userManager.DeleteAsync(user!);

        var dto = new { PhoneNumber = "+79123456789" };
        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
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
            Console.WriteLine($"[GenerateSmsMock_UserNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GenerateSmsMock_Should_ReturnMockCallback_WhenValidRequest()
    {
        // Arrange
        var (_, accessToken) = await CreateAuthenticatedUserAsync();
        var dto = new { PhoneNumber = "+79123456789" };

        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("phoneNumber", out var phone).Should().BeTrue();
            phone.GetString()!.Should().Be("+79123456789");
            json.TryGetProperty("message", out var msg).Should().BeTrue();
            msg.GetString()!.Should().Be("Mock SMS сгенерировано");
            json.TryGetProperty("token", out var token).Should().BeTrue();
            token.GetString()!.Should().MatchRegex(@"^\d{6}$");
        }
        catch (Exception)
        {
            Console.WriteLine($"[GenerateSmsMock_MockCallback] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("", "Номер телефона не может быть пустым")]
    [InlineData("12345", "Неверный формат номера телефона")]
    [InlineData("+123", "Неверный формат номера телефона")]
    [InlineData("79123456789", "Неверный формат номера телефона")]
    [InlineData("+791234567890123456", "Неверный формат номера телефона")]
    public async Task Endpoint_GenerateSmsMock_Should_ReturnValidationError_WhenInvalidPhone(string phone, string expectedMessagePart)
    {
        // Arrange
        var (_, accessToken) = await CreateAuthenticatedUserAsync();
        var dto = new { PhoneNumber = phone };

        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");

            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.EnumerateArray()
                  .Any(e => e.TryGetProperty("message", out var msg) && msg.GetString()!.Contains(expectedMessagePart))
                  .Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[GenerateSmsMock_ValidationError] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GenerateSmsMock_Should_ReturnDifferentTokens_ForDifferentUsers()
    {
        // Arrange
        var (_, token1) = await CreateAuthenticatedUserAsync();
        var (_, token2) = await CreateAuthenticatedUserAsync();
        var dto = new { PhoneNumber = "+79123456789" };

        using var client = CreateClientWithDisabledRateLimiter();

        // Act
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var resp1 = await client.PostAsJsonAsync(Endpoint, dto);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        var resp2 = await client.PostAsJsonAsync(Endpoint, dto);

        var smsToken1 = JsonDocument.Parse(await resp1.Content.ReadAsStringAsync()).RootElement.GetProperty("token").GetString()!;
        var smsToken2 = JsonDocument.Parse(await resp2.Content.ReadAsStringAsync()).RootElement.GetProperty("token").GetString()!;

        // Assert
        try
        {
            smsToken1.Should().NotBe(smsToken2, "разные пользователи получают разные токены");
        }
        catch (Exception)
        {
            Console.WriteLine($"Token1: {smsToken1}, Token2: {smsToken2}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GenerateSmsMock_Should_ReturnSameToken_ForSameUserAndPhone()
    {
        // Arrange
        var (_, accessToken) = await CreateAuthenticatedUserAsync();
        var dto = new { PhoneNumber = "+79123456789" };

        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var resp1 = await client.PostAsJsonAsync(Endpoint, dto);
        var resp2 = await client.PostAsJsonAsync(Endpoint, dto);

        var token1 = JsonDocument.Parse(await resp1.Content.ReadAsStringAsync()).RootElement.GetProperty("token").GetString()!;
        var token2 = JsonDocument.Parse(await resp2.Content.ReadAsStringAsync()).RootElement.GetProperty("token").GetString()!;

        // Assert
        try
        {
            token1.Should().Be(token2, "Identity генерирует детерминированный токен для одного пользователя и номера");
            token1.Should().MatchRegex(@"^\d{6}$");
        }
        catch (Exception)
        {
            Console.WriteLine($"Token1: {token1}, Token2: {token2}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GenerateSmsMock_Should_ReturnDifferentTokens_ForDifferentPhones()
    {
        // Arrange
        var (_, accessToken) = await CreateAuthenticatedUserAsync();
        var dto1 = new { PhoneNumber = "+79123456789" };
        var dto2 = new { PhoneNumber = "+79987654321" };

        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var resp1 = await client.PostAsJsonAsync(Endpoint, dto1);
        var resp2 = await client.PostAsJsonAsync(Endpoint, dto2);

        var token1 = JsonDocument.Parse(await resp1.Content.ReadAsStringAsync()).RootElement.GetProperty("token").GetString()!;
        var token2 = JsonDocument.Parse(await resp2.Content.ReadAsStringAsync()).RootElement.GetProperty("token").GetString()!;

        // Assert
        try
        {
            token1.Should().NotBe(token2, "разные номера телефонов дают разные токены");
        }
        catch (Exception)
        {
            Console.WriteLine($"Token1: {token1}, Token2: {token2}");
            throw;
        }
    }
}