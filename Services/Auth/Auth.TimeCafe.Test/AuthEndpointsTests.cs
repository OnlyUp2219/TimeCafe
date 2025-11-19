using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Xunit;

namespace Auth.TimeCafe.Test;

public class AuthEndpointsTests : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client;
    private readonly AuthApiFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public AuthEndpointsTests(AuthApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    record RegisterDto(string Username, string Email, string Password, string ConfirmPassword);
    record LoginDto(string Email, string Password);
    record TokensDto(string AccessToken, string RefreshToken);
    record RefreshRequest(string RefreshToken);
    record LogoutRequest(string RefreshToken);
    record RegisterResponse(string CallbackUrl);
    record ConfirmEmailRequest(string UserId, string Token);

    private async Task<TokensDto> RegisterAsync(string username, string email, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var existing = await userManager.FindByEmailAsync(email);
        if (existing == null)
        {
            var user = new IdentityUser { UserName = username, Email = email, EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to create test user: " + string.Join(",", createResult.Errors.Select(e => e.Code)));
            }
        }
        else if (!existing.EmailConfirmed)
        {
            existing.EmailConfirmed = true;
            await userManager.UpdateAsync(existing);
        }
        var loginResp = await _client.PostAsJsonAsync("/login-jwt", new LoginDto(email, password));
        loginResp.EnsureSuccessStatusCode();
        var json = await loginResp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TokensDto>(json, _jsonOptions)!;
    }

    [Fact]
    public async Task Register_ReturnsTokens()
    {
        var tokens = await RegisterAsync("user1", "user1@example.com", "P@ssw0rd!");
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));
    }

    [Fact]
    public async Task Register_Duplicate_ReturnsBadRequest()
    {
        await RegisterAsync("user2", "user2@example.com", "P@ssw0rd!");
        var resp = await _client.PostAsJsonAsync("/registerWithUsername", new RegisterDto("user2", "user2@example.com", "P@ssw0rd!", "P@ssw0rd!"));
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Login_Success_ReturnsTokens()
    {
        await RegisterAsync("user3", "user3@example.com", "P@ssw0rd!");
        var resp = await _client.PostAsJsonAsync("/login-jwt", new LoginDto("user3@example.com", "P@ssw0rd!"));
        resp.EnsureSuccessStatusCode();
        var tokens = JsonSerializer.Deserialize<TokensDto>(await resp.Content.ReadAsStringAsync(), _jsonOptions)!;
        Assert.NotNull(tokens);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsBadRequest()
    {
        var resp = await _client.PostAsJsonAsync("/login-jwt", new LoginDto("noexist@example.com", "badpass"));
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Protected_Unauthorized_WithoutToken()
    {
        var resp = await _client.GetAsync("/protected-test");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Protected_Authorized_WithBearer()
    {
        var tokens = await RegisterAsync("user4", "user4@example.com", "P@ssw0rd!");
        var req = new HttpRequestMessage(HttpMethod.Get, "/protected-test");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var resp = await _client.SendAsync(req);
        Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewTokens()
    {
        var tokens = await RegisterAsync("user5", "user5@example.com", "P@ssw0rd!");
        var resp = await _client.PostAsJsonAsync("/refresh-token-jwt", new RefreshRequest(tokens.RefreshToken));
        resp.EnsureSuccessStatusCode();
        var newTokens = JsonSerializer.Deserialize<TokensDto>(await resp.Content.ReadAsStringAsync(), _jsonOptions)!;
        Assert.False(string.IsNullOrWhiteSpace(newTokens.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(newTokens.RefreshToken));

        Assert.NotEqual(tokens.RefreshToken, newTokens.RefreshToken);
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsUnauthorized()
    {
        var resp = await _client.PostAsJsonAsync("/refresh-token-jwt", new RefreshRequest("invalid"));
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var tokens = await RegisterAsync("user6", "user6@example.com", "P@ssw0rd!");
        var logoutResp = await _client.PostAsJsonAsync("/logout", new LogoutRequest(tokens.RefreshToken));
        logoutResp.EnsureSuccessStatusCode();

        var refreshResp = await _client.PostAsJsonAsync("/refresh-token-jwt", new RefreshRequest(tokens.RefreshToken));
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, refreshResp.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutAuthorization_StillWorks()
    {
        var tokens = await RegisterAsync("user_logout_auth", "user_logout_auth@example.com", "P@ssw0rd!");

        var resp = await _client.PostAsJsonAsync("/logout", new LogoutRequest(tokens.RefreshToken));
        resp.EnsureSuccessStatusCode();
        
        var content = await resp.Content.ReadAsStringAsync();
        Assert.Contains("\"revoked\":true", content);
    }

    [Fact]
    public async Task Logout_EmptyRefreshToken_ReturnsBadRequest()
    {
        var tokens = await RegisterAsync("user_logout_empty", "user_logout_empty@example.com", "P@ssw0rd!");

        var req = new HttpRequestMessage(HttpMethod.Post, "/logout")
        {
            Content = JsonContent.Create(new LogoutRequest(""))
        };
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var resp = await _client.SendAsync(req);
        Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task Logout_NonExistentToken_ReturnsOkButNotRevoked()
    {
        var tokens = await RegisterAsync("user_logout_fake", "user_logout_fake@example.com", "P@ssw0rd!");

        var raw = "nonexistent-token-456";
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        var logoutDto = new { RefreshToken = base64 };

        var req = new HttpRequestMessage(HttpMethod.Post, "/logout")
        {
            Content = JsonContent.Create(new LogoutRequest(logoutDto.RefreshToken))
        };
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var resp = await _client.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<LogoutResponse>(json, _jsonOptions);
        Assert.NotNull(result);
        Assert.False(result.Revoked);
    }

    [Fact]
    public async Task RefreshToken_RotatesToken()
    {
        var tokens = await RegisterAsync("user_refresh_rotate", "user_refresh_rotate@example.com", "P@ssw0rd!");

        var resp = await _client.PostAsJsonAsync("/refresh-token-jwt", new RefreshRequest(tokens.RefreshToken));
        resp.EnsureSuccessStatusCode();

        var newTokens = JsonSerializer.Deserialize<TokensDto>(await resp.Content.ReadAsStringAsync(), _jsonOptions)!;
        Assert.NotEqual(tokens.RefreshToken, newTokens.RefreshToken);
        Assert.NotEqual(tokens.AccessToken, newTokens.AccessToken);
    }

    [Fact]
    public async Task RefreshToken_OldTokenBecomesInvalid()
    {
        var tokens = await RegisterAsync("user_refresh_old", "user_refresh_old@example.com", "P@ssw0rd!");

        var refreshResp = await _client.PostAsJsonAsync("/refresh-token-jwt", new RefreshRequest(tokens.RefreshToken));
        refreshResp.EnsureSuccessStatusCode();

        var oldRefreshResp = await _client.PostAsJsonAsync("/refresh-token-jwt", new RefreshRequest(tokens.RefreshToken));
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, oldRefreshResp.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_EmptyToken_ReturnsUnauthorized()
    {
        var resp = await _client.PostAsJsonAsync("/refresh-token-jwt", new RefreshRequest(""));
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task ProtectedTest_WithValidToken_ReturnsUserInfo()
    {
        var tokens = await RegisterAsync("user_protected", "user_protected@example.com", "P@ssw0rd!");

        var req = new HttpRequestMessage(HttpMethod.Get, "/protected-test");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var resp = await _client.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var content = await resp.Content.ReadAsStringAsync();
        Assert.Contains("user_protected@example.com", content);
    }

    [Fact]
    public async Task ProtectedTest_WithExpiredToken_ReturnsUnauthorized()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "/protected-test");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired.token.here");

        var resp = await _client.SendAsync(req);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    record LogoutResponse(string Message, bool Revoked);
}

