using System.IdentityModel.Tokens.Jwt;
using BuildingBlocks.Permissions;

namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class JwtClaimsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    private const string LoginEndpoint = "/auth/login-jwt";

    [Fact]
    public async Task Endpoint_Login_Should_NotIncludePermissionsClaim_InAccessToken()
    {
        var email = $"jwt_user_{Guid.NewGuid():N}@example.com";
        const string password = "P@ssw0rd!";

        using (var scope = Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            if (!await roleManager.RoleExistsAsync(Roles.Client))
            {
                var createRole = await roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Client));
                createRole.Succeeded.Should().BeTrue();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createUser = await userManager.CreateAsync(user, password);
            createUser.Succeeded.Should().BeTrue();

            var addRole = await userManager.AddToRoleAsync(user, Roles.Client);
            addRole.Succeeded.Should().BeTrue();
        }

        var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new { Email = email, Password = password });
        loginResponse.EnsureSuccessStatusCode();

        var json = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync()).RootElement;
        var accessToken = json.GetProperty("accessToken").GetString();
        accessToken.Should().NotBeNullOrWhiteSpace();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken!);
        jwt.Claims.Any(c => c.Type == CustomClaimTypes.Permissions).Should().BeFalse();
    }
}