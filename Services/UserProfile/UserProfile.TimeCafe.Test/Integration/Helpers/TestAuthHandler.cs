using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace UserProfile.TimeCafe.Test.Integration.Helpers;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "TestScheme";

    public static readonly Guid DefaultTestUserId = Auth.DefaultUserId;
    public static readonly Guid AdminUserId = Auth.AdminUserId;
    public static readonly Guid ClientUserId = Auth.ClientUserId;

    public const string TestUserId = "11111111-1111-1111-1111-111111111111";

    public static Guid CurrentUserId { get; set; } = DefaultTestUserId;
    public static string CurrentRole { get; set; } = Auth.AdminRole;

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, CurrentUserId.ToString()),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, CurrentRole)
        };

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    public static void Reset()
    {
        CurrentUserId = DefaultTestUserId;
        CurrentRole = Auth.AdminRole;
    }
}
