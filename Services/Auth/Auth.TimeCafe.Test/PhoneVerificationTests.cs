using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Auth.TimeCafe.Test;

public class PhoneVerificationTests : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client;
    private readonly AuthApiFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public PhoneVerificationTests(AuthApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    record TokensDto(string AccessToken, string RefreshToken);
    record LoginDto(string Email, string Password);
    record PhoneVerificationModel(string PhoneNumber, string Code, string? CaptchaToken);
    record GenerateSmsResponse(string PhoneNumber, string Message, string Token);
    record VerifySmsResponse(string Message, string PhoneNumber);
    record VerifySmsError(string Message, int RemainingAttempts, bool RequiresCaptcha);

    private async Task<TokensDto> RegisterAndLoginAsync(string username, string email, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var existing = await userManager.FindByEmailAsync(email);
        if (existing == null)
        {
            var user = new IdentityUser { UserName = username, Email = email, EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException("Failed to create test user");
        }

        var loginResp = await _client.PostAsJsonAsync("/login-jwt", new LoginDto(email, password));
        loginResp.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<TokensDto>(await loginResp.Content.ReadAsStringAsync(), _jsonOptions)!;
    }

    [Fact]
    public async Task GenerateSMS_Mock_ReturnsToken()
    {
        var tokens = await RegisterAndLoginAsync("phone_user1", "phone_user1@example.com", "P@ssw0rd!");

        var req = new HttpRequestMessage(HttpMethod.Post, "/twilio/generateSMS-mock")
        {
            Content = JsonContent.Create(new PhoneVerificationModel("+1234567890", "", null))
        };
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var resp = await _client.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var result = JsonSerializer.Deserialize<GenerateSmsResponse>(await resp.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.Equal("+1234567890", result.PhoneNumber);
    }

    [Fact]
    public async Task GenerateSMS_Mock_RequiresAuthorization()
    {
        var resp = await _client.PostAsJsonAsync("/twilio/generateSMS-mock", new PhoneVerificationModel("+1234567890", "", null));
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task VerifySMS_Mock_WithValidCode_Success()
    {
        var tokens = await RegisterAndLoginAsync("phone_user2", "phone_user2@example.com", "P@ssw0rd!");

        // Generate SMS
        var genReq = new HttpRequestMessage(HttpMethod.Post, "/twilio/generateSMS-mock")
        {
            Content = JsonContent.Create(new PhoneVerificationModel("+9876543210", "", null))
        };
        genReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var genResp = await _client.SendAsync(genReq);
        genResp.EnsureSuccessStatusCode();

        var genResult = JsonSerializer.Deserialize<GenerateSmsResponse>(await genResp.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(genResult?.Token);

        // Verify SMS
        var verReq = new HttpRequestMessage(HttpMethod.Post, "/twilio/verifySMS-mock")
        {
            Content = JsonContent.Create(new PhoneVerificationModel("+9876543210", genResult.Token, null))
        };
        verReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var verResp = await _client.SendAsync(verReq);
        verResp.EnsureSuccessStatusCode();

        var verResult = JsonSerializer.Deserialize<VerifySmsResponse>(await verResp.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(verResult);
        Assert.Contains("подтвержден", verResult.Message);
    }

    [Fact]
    public async Task VerifySMS_Mock_WithInvalidCode_ReturnsRemainingAttempts()
    {
        var tokens = await RegisterAndLoginAsync("phone_user3", "phone_user3@example.com", "P@ssw0rd!");

        var req = new HttpRequestMessage(HttpMethod.Post, "/twilio/verifySMS-mock")
        {
            Content = JsonContent.Create(new PhoneVerificationModel("+1111111111", "wrongcode", null))
        };
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var resp = await _client.SendAsync(req);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, resp.StatusCode);

        var error = JsonSerializer.Deserialize<VerifySmsError>(await resp.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(error);
        Assert.True(error.RemainingAttempts >= 0);
    }

    [Fact]
    public async Task VerifySMS_Mock_RequiresAuthorization()
    {
        var resp = await _client.PostAsJsonAsync("/twilio/verifySMS-mock", new PhoneVerificationModel("+1234567890", "123456", null));
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task VerifySMS_Mock_AfterMultipleFailedAttempts_RequiresCaptcha()
    {
        var tokens = await RegisterAndLoginAsync("phone_user4", "phone_user4@example.com", "P@ssw0rd!");
        var phone = "+2222222222";

        for (int i = 0; i < 3; i++)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "/twilio/verifySMS-mock")
            {
                Content = JsonContent.Create(new PhoneVerificationModel(phone, "wrongcode", null))
            };
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            await _client.SendAsync(req);
        }

        var finalReq = new HttpRequestMessage(HttpMethod.Post, "/twilio/verifySMS-mock")
        {
            Content = JsonContent.Create(new PhoneVerificationModel(phone, "wrongcode", null))
        };
        finalReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var finalResp = await _client.SendAsync(finalReq);

        var error = JsonSerializer.Deserialize<VerifySmsError>(await finalResp.Content.ReadAsStringAsync(), _jsonOptions);
        Assert.NotNull(error);
        Assert.True(error.RequiresCaptcha);
    }
}
