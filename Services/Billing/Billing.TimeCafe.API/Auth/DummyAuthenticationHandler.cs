namespace Billing.TimeCafe.API.Auth;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class DummyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DummyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "dummy-user-id"),
            new Claim(ClaimTypes.Name, "Dummy User")
        }, "DummyScheme");

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "DummyScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
