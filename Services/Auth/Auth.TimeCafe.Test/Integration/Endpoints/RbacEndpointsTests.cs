using System.Security.Claims;
using BuildingBlocks.Permissions;

namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class RbacEndpointsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    private const string LoginEndpoint = "/auth/login-jwt";
    private const string RbacRolesEndpoint = "/auth/rbac/roles";

    private async Task<string> CreateUserWithRoleAndLoginAsync(string role, IEnumerable<string>? rolePermissions = null)
    {
        var email = $"rbac_user_{Guid.NewGuid():N}@example.com";
        const string password = "P@ssw0rd!";

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var identityRole = await roleManager.FindByNameAsync(role);
        if (identityRole is null)
        {
            identityRole = new IdentityRole<Guid>(role);
            var createRole = await roleManager.CreateAsync(identityRole);
            createRole.Succeeded.Should().BeTrue();
        }

        if (rolePermissions is not null)
        {
            var existingClaims = await roleManager.GetClaimsAsync(identityRole);
            foreach (var claim in existingClaims.Where(c => c.Type == CustomClaimTypes.Permissions))
            {
                await roleManager.RemoveClaimAsync(identityRole, claim);
            }

            foreach (var permission in rolePermissions)
            {
                await roleManager.AddClaimAsync(identityRole, new Claim(CustomClaimTypes.Permissions, permission));
            }
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createUser = await userManager.CreateAsync(user, password);
        createUser.Succeeded.Should().BeTrue();

        var addRole = await userManager.AddToRoleAsync(user, role);
        addRole.Succeeded.Should().BeTrue();

        var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new { Email = email, Password = password });
        loginResponse.EnsureSuccessStatusCode();

        var json = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync()).RootElement;
        return json.GetProperty("accessToken").GetString()!;
    }

    [Fact]
    public async Task Endpoint_GetRoles_Should_Return401_WhenUnauthorized()
    {
        var response = await Client.GetAsync(RbacRolesEndpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_GetRoles_Should_Return403_WhenUserLacksRbacReadPermission()
    {
        var token = await CreateUserWithRoleAndLoginAsync(Roles.Client);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync(RbacRolesEndpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_GetRoles_Should_Return200_WhenUserHasRbacReadPermission()
    {
        var customRole = $"reader_{Guid.NewGuid():N}";
        var token = await CreateUserWithRoleAndLoginAsync(customRole, [Permissions.RbacRoleRead]);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync(RbacRolesEndpoint);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_CreateRole_Should_Return201_AndRoleShouldExist_WhenUserHasRbacCreatePermission()
    {
        var token = await CreateUserWithRoleAndLoginAsync(Roles.Admin);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var roleName = $"managed_{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync(RbacRolesEndpoint, new
        {
            roleName,
            claims = new[] { Permissions.AccountSelfRead }
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var existsResponse = await Client.GetAsync($"/auth/rbac/roles/{roleName}/exists");
        existsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = JsonDocument.Parse(await existsResponse.Content.ReadAsStringAsync()).RootElement;
        json.GetProperty("exists").GetBoolean().Should().BeTrue();
    }
}