using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class PhoneGenerateTests : BaseEndpointTest
{
    private const string Endpoint = "/twilio/generateSMS-mock";

    public PhoneGenerateTests(IntegrationApiFactory factory) : base(factory)
    {
    }

    private HttpClient CreateClientWithDisabledRateLimiter()
    {
        var overrides = new Dictionary<string, string?>
        {
            ["RateLimiter:EmailSms:MinIntervalSeconds"] = "0",
            ["RateLimiter:EmailSms:WindowMinutes"] = "1",
            ["RateLimiter:EmailSms:MaxRequests"] = "10000"
        };

        return Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, conf) => conf.AddInMemoryCollection(overrides));
        }).CreateClient();
    }

    private async Task<(string userId, string accessToken)> CreateAuthenticatedUserAsync()
    {
        var email = $"user_{Guid.NewGuid():N}@example.com";
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        await userManager.CreateAsync(user, "P@ssw0rd!");

        var loginDto = new { Email = email, Password = "P@ssw0rd!" };
        var loginResp = await Client.PostAsJsonAsync("/login-jwt", loginDto);
        loginResp.EnsureSuccessStatusCode();
        var json = JsonDocument.Parse(await loginResp.Content.ReadAsStringAsync()).RootElement;
        var token = json.GetProperty("accessToken").GetString()!;

        return (user.Id, token);
    }

    [Fact]
    public async Task Endpoint_GenerateSmsMock_Should_ReturnUnauthorized_WhenNoToken()
    {
        // Arrange
        var dto = new { PhoneNumber = "+79123456789" };
        using var client = CreateClientWithDisabledRateLimiter();

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_GenerateSmsMock_Should_ReturnUserNotFound_WhenUserNotInDb()
    {
        // Arrange — токен валидный, но пользователя нет (удаляем после логина)
        var (userId, accessToken) = await CreateAuthenticatedUserAsync();

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByIdAsync(userId);
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
            code.GetString().Should().Be("UserNotFound");
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
            phone.GetString().Should().Be("+79123456789");
            json.TryGetProperty("message", out var msg).Should().BeTrue();
            msg.GetString().Should().Be("Mock SMS сгенерировано");
            json.TryGetProperty("token", out var token).Should().BeTrue();
            token.GetString().Should().MatchRegex(@"^\d{6}$");
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
            code.GetString().Should().Be("ValidationError");

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
    public async Task Endpoint_GenerateSmsMock_Should_GenerateDifferentToken_OnEachCall()
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
            token1.Should().NotBe(token2, "токены должны быть разными при каждом вызове");
            token1.Should().MatchRegex(@"^\d{6}$");
            token2.Should().MatchRegex(@"^\d{6}$");
        }
        catch (Exception)
        {
            Console.WriteLine($"Token1: {token1}, Token2: {token2}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GenerateSmsMock_Should_WorkWithRateLimiterOverrides()
    {
        // Arrange
        var (_, accessToken) = await CreateAuthenticatedUserAsync();
        var dto = new { PhoneNumber = "+79123456789" };

        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 20; i++)
        {
            responses.Add(await client.PostAsJsonAsync(Endpoint, dto));
        }

        // Assert
        try
        {
            responses.All(r => r.StatusCode == HttpStatusCode.OK).Should().BeTrue("RateLimiter полностью отключён");
        }
        catch (Exception)
        {
            var failed = responses.Count(r => r.StatusCode != HttpStatusCode.OK);
            Console.WriteLine($"Failed: {failed} из {responses.Count}");
            throw;
        }
    }
}