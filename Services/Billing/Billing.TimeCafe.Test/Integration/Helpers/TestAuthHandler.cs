using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using BuildingBlocks.Permissions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Billing.TimeCafe.Test.Integration.Helpers;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "TestScheme";

    public static readonly Guid DefaultTestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AdminUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ClientUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public const string TestUserId = "11111111-1111-1111-1111-111111111111";

    public static Guid CurrentUserId { get; set; } = DefaultTestUserId;
    public static string CurrentRole { get; set; } = "Admin";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, CurrentUserId.ToString()),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@example.com"),
            new(ClaimTypes.Role, CurrentRole)
        };

        var permissions = typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
            .Select(field => (string)field.GetValue(null)!)
            .Distinct(StringComparer.Ordinal);

        claims.AddRange(permissions.Select(permission => new Claim(CustomClaimTypes.Permissions, permission)));

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    public static void Reset()
    {
        CurrentUserId = DefaultTestUserId;
        CurrentRole = "Admin";
    }
}




