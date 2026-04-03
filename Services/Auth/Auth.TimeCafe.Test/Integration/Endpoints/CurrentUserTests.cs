namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class CurrentUserTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    private const string LoginEndpoint = "/auth/login-jwt";
    private const string CurrentUserEndpoint = "/auth/account/me";
    private const string CurrentAdminUserEndpoint = "/auth/account/admin/me";

    private async Task<string> CreateUserWithRoleAndLoginAsync(string role)
    {
        var email = $"current_user_{Guid.NewGuid():N}@example.com";
        const string password = "P@ssw0rd!";

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, password);
        createResult.Succeeded.Should().BeTrue();

        var addRoleResult = await userManager.AddToRoleAsync(user, role);
        addRoleResult.Succeeded.Should().BeTrue();

        var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new { Email = email, Password = password });
        loginResponse.EnsureSuccessStatusCode();

        var json = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync()).RootElement;
        return json.GetProperty("accessToken").GetString()!;
    }

    [Fact]
    public async Task Endpoint_GetCurrentUser_Should_Return401_WhenUnauthorized()
    {
        var response = await Client.GetAsync(CurrentUserEndpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoint_GetCurrentUser_Should_Return200_WhenUserHasClientReadPermission()
    {
        var accessToken = await CreateUserWithRoleAndLoginAsync(BuildingBlocks.Permissions.Roles.Client);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await Client.GetAsync(CurrentUserEndpoint);
        var jsonString = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = JsonDocument.Parse(jsonString).RootElement;
        json.TryGetProperty("userId", out var userId).Should().BeTrue();
        userId.GetString().Should().NotBeNullOrWhiteSpace();
        json.TryGetProperty("email", out var email).Should().BeTrue();
        email.GetString().Should().Contain("current_user_");
    }

    [Fact]
    public async Task Endpoint_GetCurrentAdminUser_Should_Return403_WhenUserLacksAdminPermission()
    {
        var accessToken = await CreateUserWithRoleAndLoginAsync(BuildingBlocks.Permissions.Roles.Client);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await Client.GetAsync(CurrentAdminUserEndpoint);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_GetCurrentAdminUser_Should_Return200_WhenUserHasAdminPermission()
    {
        var accessToken = await CreateUserWithRoleAndLoginAsync(BuildingBlocks.Permissions.Roles.Admin);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await Client.GetAsync(CurrentAdminUserEndpoint);
        var jsonString = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = JsonDocument.Parse(jsonString).RootElement;
        json.TryGetProperty("userId", out var userId).Should().BeTrue();
        userId.GetString().Should().NotBeNullOrWhiteSpace();
        json.TryGetProperty("email", out var email).Should().BeTrue();
        email.GetString().Should().Contain("current_user_");
    }
}
