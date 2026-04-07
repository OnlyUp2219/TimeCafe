using System.Security.Claims;
using BuildingBlocks.Permissions;

namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class RbacEndpointsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    private const string LoginEndpoint = "/auth/login-jwt";
    private const string RbacRolesEndpoint = "/auth/rbac/roles";

    private async Task EnsureRoleWithPermissionsAsync(string roleName, IEnumerable<string>? rolePermissions)
    {
        using var scope = Factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var identityRole = await roleManager.FindByNameAsync(roleName);
        if (identityRole is null)
        {
            identityRole = new IdentityRole<Guid>(roleName);
            var createRole = await roleManager.CreateAsync(identityRole);
            createRole.Succeeded.Should().BeTrue();
        }

        if (rolePermissions is null)
            return;

        var existingClaims = await roleManager.GetClaimsAsync(identityRole);
        foreach (var claim in existingClaims.Where(c => c.Type == CustomClaimTypes.Permissions))
        {
            await roleManager.RemoveClaimAsync(identityRole, claim);
        }

        foreach (var permission in rolePermissions.Distinct(StringComparer.Ordinal))
        {
            await roleManager.AddClaimAsync(identityRole, new Claim(CustomClaimTypes.Permissions, permission));
        }
    }

    private async Task<(Guid userId, string accessToken)> CreateUserAndLoginAsync(string? roleName = null, IEnumerable<string>? rolePermissions = null)
    {
        var email = $"rbac_user_{Guid.NewGuid():N}@example.com";
        const string password = "P@ssw0rd!";

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (!string.IsNullOrWhiteSpace(roleName))
        {
            await EnsureRoleWithPermissionsAsync(roleName, rolePermissions);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createUser = await userManager.CreateAsync(user, password);
        createUser.Succeeded.Should().BeTrue();

        if (!string.IsNullOrWhiteSpace(roleName))
        {
            var addRole = await userManager.AddToRoleAsync(user, roleName);
            addRole.Succeeded.Should().BeTrue();
        }

        var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new { Email = email, Password = password });
        loginResponse.EnsureSuccessStatusCode();

        var json = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync()).RootElement;
        var accessToken = json.GetProperty("accessToken").GetString()!;
        return (user.Id, accessToken);
    }

    private async Task<string> CreateUserWithRoleAndLoginAsync(string role, IEnumerable<string>? rolePermissions = null)
    {
        var (_, token) = await CreateUserAndLoginAsync(role, rolePermissions);
        return token;
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

    [Fact]
    public async Task Endpoint_AssignRoleToUser_Should_GrantPermission_WhenCacheAlreadyContainsDeniedSnapshot()
    {
        var adminToken = await CreateUserWithRoleAndLoginAsync(Roles.Admin);

        var assignableRole = $"assignable_{Guid.NewGuid():N}";
        await EnsureRoleWithPermissionsAsync(assignableRole, [Permissions.RbacRoleRead]);

        var (userId, userToken) = await CreateUserAndLoginAsync();

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var deniedBeforeAssign = await Client.GetAsync(RbacRolesEndpoint);
        deniedBeforeAssign.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var assignResponse = await Client.PostAsync($"/auth/rbac/users/{userId}/roles/{assignableRole}", content: null);
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var allowedAfterAssign = await Client.GetAsync(RbacRolesEndpoint);
        allowedAfterAssign.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_RemoveRoleFromUser_Should_RevokePermission_WithSameToken()
    {
        var adminToken = await CreateUserWithRoleAndLoginAsync(Roles.Admin);

        var removableRole = $"removable_{Guid.NewGuid():N}";
        var (userId, userToken) = await CreateUserAndLoginAsync(removableRole, [Permissions.RbacRoleRead]);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var allowedBeforeRemove = await Client.GetAsync(RbacRolesEndpoint);
        allowedBeforeRemove.StatusCode.Should().Be(HttpStatusCode.OK);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var removeResponse = await Client.DeleteAsync($"/auth/rbac/users/{userId}/roles/{removableRole}");
        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var deniedAfterRemove = await Client.GetAsync(RbacRolesEndpoint);
        deniedAfterRemove.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_UpdateRoleClaims_Should_InvalidateRoleTagCache_ForAssignedUsers()
    {
        var adminToken = await CreateUserWithRoleAndLoginAsync(Roles.Admin);

        var mutableRole = $"mutable_{Guid.NewGuid():N}";
        var (_, userToken) = await CreateUserAndLoginAsync(mutableRole, [Permissions.RbacRoleRead]);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var allowedBeforeUpdate = await Client.GetAsync(RbacRolesEndpoint);
        allowedBeforeUpdate.StatusCode.Should().Be(HttpStatusCode.OK);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var updateResponse = await Client.PutAsJsonAsync($"/auth/rbac/role-claims/{mutableRole}", new
        {
            claims = new[] { Permissions.AccountSelfRead }
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var deniedAfterUpdate = await Client.GetAsync(RbacRolesEndpoint);
        deniedAfterUpdate.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}