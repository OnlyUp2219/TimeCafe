
namespace Auth.TimeCafe.Test.Integration.Repository;

public class RbacCacheTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Cache_ShouldBeInvalidated_OnUpdateRoleClaims()
    {
        var (_, accessToken) = await CreateUserAndLoginAsync("CacheAdmin", [Permissions.RbacRoleCreate, Permissions.RbacRoleClaimsUpdate, Permissions.RbacRoleRead]);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        
        // Arrange
        var roleName = "CacheTestRole";
        var claims = new List<string> { "TestPermission1" };

        var createPayload = new { roleName, claims = new List<string>() };
        var createResponse = await Client.PostAsJsonAsync("/auth/rbac/roles", createPayload);
        createResponse.EnsureSuccessStatusCode();

        var initialPutPayload = new { claims };
        var initialPutResponse = await Client.PutAsJsonAsync($"/auth/rbac/role-claims/{roleName}", initialPutPayload);
        initialPutResponse.EnsureSuccessStatusCode();

        // 2. Получить данные (GET)
        var initialGetResponse = await Client.GetAsync($"/auth/rbac/role-claims/{roleName}");
        initialGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var initialJsonStr = await initialGetResponse.Content.ReadAsStringAsync();
        var initialClaims = JsonDocument.Parse(initialJsonStr).RootElement.GetProperty("roleClaim").GetProperty("claims").EnumerateArray().Select(x => x.GetString()).ToList();
        
        initialClaims.Should().Contain("TestPermission1");
        initialClaims.Should().NotContain("TestPermission2");

        // 3. Обновление (PUT /auth/rbac/role-claims)
        var updatePayload = new
        {
            claims = new List<string> { "TestPermission1", "TestPermission2" }
        };

        var updateResponse = await Client.PutAsJsonAsync($"/auth/rbac/role-claims/{roleName}", updatePayload);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Получить данные (GET after PUT)
        // Кэш должен был инвалидироваться, и мы должны увидеть новое разрешение
        var getAfterUpdateResponse = await Client.GetAsync($"/auth/rbac/role-claims/{roleName}");
        getAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getAfterUpdateJsonStr = await getAfterUpdateResponse.Content.ReadAsStringAsync();
        var updatedClaims = JsonDocument.Parse(getAfterUpdateJsonStr).RootElement.GetProperty("roleClaim").GetProperty("claims").EnumerateArray().Select(x => x.GetString()).ToList();
        
        updatedClaims.Should().Contain("TestPermission1");
        updatedClaims.Should().Contain("TestPermission2", "Разрешения роли должны были обновиться, кэш инвалидирован");
    }
}
