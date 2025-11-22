
namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class ChangePasswordTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    private const string Endpoint = "/account/change-password";
    private const string LoginEndpoint = "/login-jwt";

    private async Task<(string email, string oldPassword, string accessToken, string refreshToken)> CreateUserAndLoginAsync()
    {
        var email = $"user_{Guid.NewGuid():N}@example.com";
        var oldPassword = "OldP@ssw0rd!";

        // создаём пользователя
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        await userManager.CreateAsync(user, oldPassword);

        // логинимся и получаем оба токена
        var loginDto = new { Email = email, Password = oldPassword };
        var loginResp = await Client.PostAsJsonAsync(LoginEndpoint, loginDto);
        loginResp.EnsureSuccessStatusCode();

        var json = JsonDocument.Parse(await loginResp.Content.ReadAsStringAsync()).RootElement;
        var accessToken = json.GetProperty("accessToken").GetString()!;
        var refreshToken = json.GetProperty("refreshToken").GetString()!;

        return (email, oldPassword, accessToken, refreshToken);
    }

    [Fact]
    public async Task Endpoint_ChangePassword_Should_ReturnUnauthorized_WhenInvalidToken()
    {
        // Arrange
        var dto = new { CurrentPassword = "OldP@ssw0rd!", NewPassword = "NewP@ssw0rd!" };
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ChangePassword_Should_ReturnUnauthorized_WhenInvalidToken] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("WrongCurrentP@ss!", "NewP@ssw0rd!", "PasswordMismatch", null)]
    [InlineData("OldP@ssw0rd!", "short", "PasswordTooShort", "Пароль должен содержать не менее")]
    [InlineData("OldP@ssw0rd!", "NoDigitsPass", "PasswordRequiresDigit", "Пароль должен содержать хотя бы одну цифру")]
    public async Task Endpoint_ChangePassword_Should_ReturnChangePasswordFailed_WhenInvalidPasswords(string current, string newPass, string expectedErrorCode, string? expectedDescriptionPart)
    {
        // Arrange
        var (_, oldPassword, accessToken, _) = await CreateUserAndLoginAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var dto = new { CurrentPassword = current == "OldP@ssw0rd!" ? oldPassword : current, NewPassword = newPass };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ChangePasswordFailed");
            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            var matchingError = errors.EnumerateArray().FirstOrDefault(e => e.TryGetProperty("code", out var errCode) && errCode.GetString() == expectedErrorCode);
            matchingError.ValueKind.Should().NotBe(JsonValueKind.Undefined);

            if (expectedDescriptionPart != null && matchingError.TryGetProperty("description", out var desc))
            {
                desc.GetString()!.Should().Contain(expectedDescriptionPart);
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ChangePassword_Should_ReturnChangePasswordFailed_WhenInvalidPasswords] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ChangePassword_Should_ReturnSuccess_WhenValidPasswords()
    {
        // Arrange
        var (_, oldPassword, accessToken, _) = await CreateUserAndLoginAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var dto = new { CurrentPassword = oldPassword, NewPassword = "NewP@ssw0rd!" };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString()!.Should().Be("Пароль изменён");
            json.TryGetProperty("refreshTokensRevoked", out var revoked).Should().BeTrue();
            revoked.GetInt32().Should().BeGreaterThan(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ChangePassword_Should_ReturnSuccess_WhenValidPasswords] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("SameP@ssw0rd!", "SameP@ssw0rd!", "Новый пароль не должен совпадать со старым")]
    public async Task Endpoint_ChangePassword_Should_ReturnValidationError_WhenSamePassword(string current, string newPass, string expectedMessagePart)
    {
        // Arrange
        var (_, _, accessToken, _) = await CreateUserAndLoginAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var dto = new { CurrentPassword = current, NewPassword = newPass };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, dto);
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
            Console.WriteLine($"[Endpoint_ChangePassword_Should_ReturnValidationError_WhenSamePassword] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ChangePassword_Should_ReturnUnauthorized_WhenNoAuthorization()
    {
        // Arrange
        var dto = new { CurrentPassword = "OldP@ssw0rd!", NewPassword = "NewP@ssw0rd!" };
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ChangePassword_Should_ReturnUnauthorized_WhenNoAuthorization] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ChangePassword_Should_RevokeMultipleTokens_AfterSuccess()
    {
        // Arrange
        var (email, oldPassword, accessToken, _) = await CreateUserAndLoginAsync();

        var loginDto = new { Email = email, Password = oldPassword };
        var loginResp1 = await Client.PostAsJsonAsync(LoginEndpoint, loginDto);
        var refreshToken1 = JsonDocument.Parse(await loginResp1.Content.ReadAsStringAsync()).RootElement.GetProperty("refreshToken").GetString();

        var refreshDto = new { RefreshToken = refreshToken1 };
        var refreshResp = await Client.PostAsJsonAsync("/refresh-token-jwt", refreshDto);
        refreshResp.EnsureSuccessStatusCode();

        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var dto = new { CurrentPassword = oldPassword, NewPassword = "NewP@ssw0rd!" };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("refreshTokensRevoked", out var revoked).Should().BeTrue();
            revoked.GetInt32().Should().BeGreaterThanOrEqualTo(2);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ChangePassword_Should_RevokeMultipleTokens_AfterSuccess] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ChangePassword_Should_ReturnSuccessWithZeroRevoked_WhenNoRefreshTokens()
    {
        // Arrange
        var (_, oldPassword, accessToken, refreshToken) = await CreateUserAndLoginAsync();
        await Client.PostAsJsonAsync("/logout", new { RefreshToken = refreshToken });

        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var dto = new { CurrentPassword = oldPassword, NewPassword = "NewP@ssw0rd!" };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("refreshTokensRevoked", out var revoked).Should().BeTrue();
            revoked.GetInt32().Should().Be(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ChangePassword_Should_ReturnSuccessWithZeroRevoked_WhenNoRefreshTokens] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ChangePassword_Should_FailConcurrent_WhenOldPasswordUsedAgain()
    {
        // Arrange
        var (_, oldPassword, accessToken, _) = await CreateUserAndLoginAsync();
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var dto = new { CurrentPassword = oldPassword, NewPassword = "NewP@ssw0rd!" };

        // Act
        var firstResponse = await Client.PostAsJsonAsync(Endpoint, dto);
        firstResponse.EnsureSuccessStatusCode();

        var secondResponse = await Client.PostAsJsonAsync(Endpoint, dto);
        var jsonString = await secondResponse.Content.ReadAsStringAsync();

        // Assert
        try
        {
            secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ChangePasswordFailed");
            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.EnumerateArray().Any(e => e.TryGetProperty("code", out var errCode) && errCode.GetString() == "PasswordMismatch").Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ChangePassword_Should_FailConcurrent_WhenOldPasswordUsedAgain] Response: {jsonString}");
            throw;
        }
    }
}