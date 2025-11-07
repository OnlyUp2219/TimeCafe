using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
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
        // Direct user creation via DI to avoid rate limiting & email flow
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

        // Login via public endpoint to get tokens (ensures auth pipeline exercised)
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
        // Refresh token should rotate
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
}

