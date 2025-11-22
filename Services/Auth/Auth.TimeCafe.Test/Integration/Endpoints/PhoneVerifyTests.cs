using Auth.TimeCafe.Domain.Contracts;

using System.Net.Http.Headers;

namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class PhoneVerifyTests : BaseEndpointTest
{
    private const string Endpoint = "/twilio/verifySMS-mock";

    public PhoneVerifyTests(IntegrationApiFactory factory) : base(factory)
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

    private async Task<string> GenerateMockCodeAsync(string userId, string accessToken, string phone)
    {
        var dto = new { PhoneNumber = phone };
        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var resp = await client.PostAsJsonAsync("/twilio/generateSMS-mock", dto);
        resp.EnsureSuccessStatusCode();
        var json = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        return json.GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task Endpoint_VerifySmsMock_Should_ReturnUserNotFound_WhenUserNotExist()
    {
        // Arrange
        var (userId, accessToken) = await CreateAuthenticatedUserAsync();
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByIdAsync(userId);
        await userManager.DeleteAsync(user);

        var dto = new { PhoneNumber = "+79123456789", Code = "123456", CaptchaToken = (string?)null };
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
            Console.WriteLine($"[VerifySmsMock_UserNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_VerifySmsMock_Should_ReturnTooManyAttempts_AfterMaxFails()
    {
        // Arrange
        var (userId, accessToken) = await CreateAuthenticatedUserAsync();
        var phone = "+79123456789";
        var invalidCode = "999999";
        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act 
        for (int i = 0; i < 5; i++)
        {
            var dto = new { PhoneNumber = phone, Code = invalidCode, CaptchaToken = (string?)null };
            await client.PostAsJsonAsync(Endpoint, dto);
        }
        var finalDto = new { PhoneNumber = phone, Code = invalidCode, CaptchaToken = (string?)null };
        var response = await client.PostAsJsonAsync(Endpoint, finalDto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("TooManyAttempts");
        }
        catch (Exception)
        {
            Console.WriteLine($"[VerifySmsMock_TooManyAttempts] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_VerifySmsMock_Should_ReturnCaptchaRequired_WhenAttemptsLow()
    {
        // Arrange
        var (userId, accessToken) = await CreateAuthenticatedUserAsync();
        var phone = "+79123456789";
        var invalidCode = "999999";
        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act 
        for (int i = 0; i < 2; i++)
        {
            var dto = new { PhoneNumber = phone, Code = invalidCode, CaptchaToken = (string?)null };
            await client.PostAsJsonAsync(Endpoint, dto);
        }
        var dtoCaptcha = new { PhoneNumber = phone, Code = invalidCode, CaptchaToken = (string?)null };
        var response = await client.PostAsJsonAsync(Endpoint, dtoCaptcha);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("CaptchaRequired");
            json.TryGetProperty("requiresCaptcha", out var requiresCaptcha).Should().BeTrue();
            requiresCaptcha.GetBoolean().Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[VerifySmsMock_CaptchaRequired] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_VerifySmsMock_Should_ReturnCaptchaInvalid_WhenCaptchaWrong()
    {
        // Arrange 
        var factoryWithMockCaptcha = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<ICaptchaValidator>(_ => new FakeCaptchaValidator { IsValid = false });
            });
        });
        var client = factoryWithMockCaptcha.CreateClient();

        var (userId, accessToken) = await CreateAuthenticatedUserAsync();
        var phone = "+79123456789";
        var invalidCode = "999999";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act 
        for (int i = 0; i < 2; i++)
        {
            var dto = new { PhoneNumber = phone, Code = invalidCode, CaptchaToken = (string?)null };
            await client.PostAsJsonAsync(Endpoint, dto);
        }
        var dtoCaptcha = new { PhoneNumber = phone, Code = invalidCode, CaptchaToken = "invalid-captcha" };
        var response = await client.PostAsJsonAsync(Endpoint, dtoCaptcha);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("CaptchaInvalid");
        }
        catch (Exception)
        {
            Console.WriteLine($"[VerifySmsMock_CaptchaInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_VerifySmsMock_Should_ReturnSuccess_WhenValidCode()
    {
        // Arrange
        var (userId, accessToken) = await CreateAuthenticatedUserAsync();
        var phone = "+79123456789";
        var code = await GenerateMockCodeAsync(userId, accessToken, phone);
        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var dto = new { PhoneNumber = phone, Code = code, CaptchaToken = (string?)null };
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString().Should().Contain("подтвержден (mock)");
        }
        catch (Exception)
        {
            Console.WriteLine($"[VerifySmsMock_SuccessResult] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_VerifySmsMock_Should_ReturnInvalidCode_WhenWrongCode()
    {
        // Arrange
        var (userId, accessToken) = await CreateAuthenticatedUserAsync();
        var phone = "+79123456789";
        await GenerateMockCodeAsync(userId, accessToken, phone);
        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var dto = new { PhoneNumber = phone, Code = "wrongcode", CaptchaToken = (string?)null };
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("InvalidCode");
            json.TryGetProperty("remainingAttempts", out var remaining).Should().BeTrue();
            remaining.GetInt32().Should().BeLessThan(5);
        }
        catch (Exception)
        {
            Console.WriteLine($"[VerifySmsMock_InvalidCode] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_VerifySmsMock_Should_ResetAttempts_OnSuccess()
    {
        // Arrange
        var (userId, accessToken) = await CreateAuthenticatedUserAsync();
        var phone = "+79123456789";
        var code = await GenerateMockCodeAsync(userId, accessToken, phone);
        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var dto = new { PhoneNumber = phone, Code = code, CaptchaToken = (string?)null };
        var response = await client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert 
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("remainingAttempts", out var remaining).Should().BeTrue();
            remaining.GetInt32().Should().Be(5);
        }
        catch (Exception)
        {
            Console.WriteLine($"[VerifySmsMock_ResetAttempts] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_VerifySmsMock_Should_RequireAuthorization()
    {
        // Arrange
        var dto = new { PhoneNumber = "+79123456789", Code = "123456", CaptchaToken = (string?)null };
        using var client = CreateClientWithDisabledRateLimiter();

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_VerifySmsMock_Should_DecreaseAttempts_OnFail()
    {
        // Arrange
        var (userId, accessToken) = await CreateAuthenticatedUserAsync();
        var phone = "+79123456789";
        await GenerateMockCodeAsync(userId, accessToken, phone);
        using var client = CreateClientWithDisabledRateLimiter();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act - first fail
        var dto1 = new { PhoneNumber = phone, Code = "wrongcode", CaptchaToken = (string?)null };
        var resp1 = await client.PostAsJsonAsync(Endpoint, dto1);
        var json1 = JsonDocument.Parse(await resp1.Content.ReadAsStringAsync()).RootElement.GetProperty("remainingAttempts").GetInt32();

        // second fail
        var dto2 = new { PhoneNumber = phone, Code = "wrongcode", CaptchaToken = (string?)null };
        var resp2 = await client.PostAsJsonAsync(Endpoint, dto2);
        var json2 = JsonDocument.Parse(await resp2.Content.ReadAsStringAsync()).RootElement.GetProperty("remainingAttempts").GetInt32();

        // Assert
        try
        {
            json2.Should().Be(json1 - 1);
        }
        catch (Exception)
        {
            Console.WriteLine($"Remaining1: {json1}, Remaining2: {json2}");
            throw;
        }
    }
}

internal class FakeCaptchaValidator : ICaptchaValidator
{
    public bool IsValid { get; set; } = true;

    public Task<bool> ValidateAsync(string? captchaToken)
    {
        return Task.FromResult(IsValid);
    }
}