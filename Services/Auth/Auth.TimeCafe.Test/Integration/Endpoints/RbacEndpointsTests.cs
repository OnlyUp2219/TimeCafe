using System.Security.Claims;
using BuildingBlocks.Permissions;

namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class RbacEndpointsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    private const string LoginEndpoint = "/auth/login-jwt";
    private const string RbacRolesEndpoint = "/auth/rbac/roles";



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
        var token = await CreateUserWithRoleAndLoginAsync(Roles.Admin, [Permissions.RbacRoleCreate, Permissions.RbacRoleRead]);
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
        var adminToken = await CreateUserWithRoleAndLoginAsync(Roles.Admin, [Permissions.RbacUserRoleAssign, Permissions.RbacRoleRead]);

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
        var adminToken = await CreateUserWithRoleAndLoginAsync(Roles.Admin, [Permissions.RbacUserRoleRemove, Permissions.RbacRoleRead, Permissions.RbacSuperAdmin]);

        var removableRole = $"removable_{Guid.NewGuid():N}";
        var (userId, userToken) = await CreateUserAndLoginAsync(removableRole, [Permissions.RbacRoleRead]);

        using (var scope = Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            await userManager.AddToRoleAsync(user!, Roles.Client);
        }

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
        var adminToken = await CreateUserWithRoleAndLoginAsync(Roles.Admin, [Permissions.RbacRoleClaimsUpdate, Permissions.RbacRoleRead]);

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

    [Fact]
    public async Task Endpoint_AssignSuperAdmin_Should_Return403_WhenUserIsNotSuperAdmin()
    {
        var adminToken = await CreateUserWithRoleAndLoginAsync(Roles.Admin, [Permissions.RbacUserRoleAssign]);
        var (targetUserId, _) = await CreateUserAndLoginAsync(Roles.Client);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await Client.PostAsync($"/auth/rbac/users/{targetUserId}/roles/{Roles.SuperAdmin}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_UpdateSuperAdminClaims_Should_Return403_BecauseItIsSystemRole()
    {
        var adminToken = await CreateUserWithRoleAndLoginAsync(Roles.Admin, [Permissions.RbacRoleClaimsUpdate, Permissions.RbacSuperAdmin]);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await Client.PutAsJsonAsync($"/auth/rbac/role-claims/{Roles.SuperAdmin}", new
        {
            claims = new[] { Permissions.RbacSuperAdmin }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_DeleteSystemRole_Should_Return403()
    {
        var adminToken = await CreateUserWithRoleAndLoginAsync(Roles.Admin, [Permissions.RbacRoleDelete, Permissions.RbacSuperAdmin]);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await Client.DeleteAsync($"/auth/rbac/roles/{Roles.Admin}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
