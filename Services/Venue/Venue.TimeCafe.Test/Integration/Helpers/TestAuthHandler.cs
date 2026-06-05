using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using BuildingBlocks.Permissions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Venue.TimeCafe.Test.Integration.Helpers;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "TestScheme";
    public const string TestUserId = "11111111-1111-1111-1111-111111111111";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = TestUserId;
        var role = "admin";

        if (Request.Headers.TryGetValue("X-Test-UserId", out var headerUserId) && Guid.TryParse(headerUserId, out var parsedUserId))
        {
            userId = parsedUserId.ToString();
        }
        if (Request.Headers.TryGetValue("X-Test-UserRole", out var headerUserRole))
        {
            role = headerUserRole.ToString();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("sub", userId),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@example.com"),
            new(ClaimTypes.Role, role)
        };

        if (role == "admin")
        {
            var permissions = typeof(Permissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                .Select(field => (string)field.GetValue(null)!)
                .Distinct(StringComparer.Ordinal);

            claims.AddRange(permissions.Select(permission => new Claim(CustomClaimTypes.Permissions, permission)));
        }
        else
        {
            claims.Add(new Claim(CustomClaimTypes.Permissions, Permissions.VenueVisitCreate));
            claims.Add(new Claim(CustomClaimTypes.Permissions, Permissions.VenueVisitEnd));
            claims.Add(new Claim(CustomClaimTypes.Permissions, Permissions.VenueVisitRead));
        }

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}







