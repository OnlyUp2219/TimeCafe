namespace UserProfile.TimeCafe.Test.Integration;

public class AuthorizationEndpointTests : IClassFixture<IntegrationApiFactory>, IDisposable
{
    private readonly IntegrationApiFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _adminUserId = Auth.AdminUserId;
    private readonly Guid _clientUserId = Auth.ClientUserId;
    private readonly Guid _otherUserId = Auth.OtherUserId;

    public AuthorizationEndpointTests(IntegrationApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
        _factory.PermissionService.Clear();
        _factory.PermissionService.SetAsAdmin(_adminUserId);
        _factory.PermissionService.SetAsClient(_clientUserId);
        TestAuthHandler.Reset();
    }

    public void Dispose()
    {
        TestAuthHandler.Reset();
        _factory.PermissionService.Clear();
    }

    [Fact]
    public async Task Endpoint_AdminOnly_Should_Return200_WhenUserHasAdminViewPermission()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _adminUserId;
        TestAuthHandler.CurrentRole = Auth.AdminRole;

        // Act
        var response = await _client.GetAsync("/test/auth/admin-only");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<TestResponse>();
        content!.Endpoint.Should().Be("admin-only");
        content.UserId.Should().Be(_adminUserId.ToString());
    }

    [Fact]
    public async Task Endpoint_AdminOnly_Should_Return403_WhenUserLacksAdminViewPermission()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _clientUserId;
        TestAuthHandler.CurrentRole = Auth.ClientRole;

        // Act
        var response = await _client.GetAsync("/test/auth/admin-only");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_AdminOnly_Should_Return401_WhenUserNotAuthenticated()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _otherUserId;
        TestAuthHandler.CurrentRole = "none";

        // Act
        var response = await _client.GetAsync("/test/auth/admin-only");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_ClientOrAdmin_Should_Return200_WhenUserHasClientViewPermission()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _clientUserId;
        TestAuthHandler.CurrentRole = Auth.ClientRole;

        // Act
        var response = await _client.GetAsync("/test/auth/client-or-admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_ClientOrAdmin_Should_Return200_WhenUserHasAdminViewPermission()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _adminUserId;
        TestAuthHandler.CurrentRole = Auth.AdminRole;

        // Act
        var response = await _client.GetAsync("/test/auth/client-or-admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_ClientOrAdmin_Should_Return403_WhenUserHasNeitherPermission()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _otherUserId;
        TestAuthHandler.CurrentRole = Auth.GuestRole;

        // Act
        var response = await _client.GetAsync("/test/auth/client-or-admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_MultiPermission_Should_Return200_WhenUserHasBothPermissions()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _clientUserId;
        TestAuthHandler.CurrentRole = Auth.ClientRole;

        // Act
        var response = await _client.GetAsync("/test/auth/multi-permission");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_MultiPermission_Should_Return403_WhenUserHasOnlyOnePermission()
    {
        // Arrange
        var partialUserId = Guid.NewGuid();
        _factory.PermissionService.SetPermissions(partialUserId, Permission.ClientView);
        TestAuthHandler.CurrentUserId = partialUserId;
        TestAuthHandler.CurrentRole = "partial";

        // Act
        var response = await _client.GetAsync("/test/auth/multi-permission");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_IdorProfile_Should_Return200_WhenUserAccessesOwnProfile()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _clientUserId;
        TestAuthHandler.CurrentRole = Auth.ClientRole;

        // Act
        var response = await _client.GetAsync($"/test/auth/profile/{_clientUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<IdorTestResponse>();
        content!.IsOwner.Should().BeTrue();
        content.Message.Should().Contain("своего профиля");
    }

    [Fact]
    public async Task Endpoint_IdorProfile_Should_Return200_WhenAdminAccessesOtherProfile()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _adminUserId;
        TestAuthHandler.CurrentRole = Auth.AdminRole;

        // Act
        var response = await _client.GetAsync($"/test/auth/profile/{_clientUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<IdorTestResponse>();
        content!.IsOwner.Should().BeFalse();
        content.Message.Should().Contain("чужого профиля");
    }

    [Fact]
    public async Task Endpoint_IdorProfile_Should_Return403_WhenClientAccessesOtherProfile()
    {
        // Arrange
        var restrictedClientId = Guid.NewGuid();
        _factory.PermissionService.SetPermissions(restrictedClientId);
        TestAuthHandler.CurrentUserId = restrictedClientId;
        TestAuthHandler.CurrentRole = Auth.ClientRole;

        // Act
        var response = await _client.GetAsync($"/test/auth/profile/{_otherUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_IdorProfileEdit_Should_Return200_WhenUserEditsOwnProfile()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _clientUserId;
        TestAuthHandler.CurrentRole = Auth.ClientRole;

        // Act
        var response = await _client.PutAsync($"/test/auth/profile/{_clientUserId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<IdorTestResponse>();
        content!.IsOwner.Should().BeTrue();
    }

    [Fact]
    public async Task Endpoint_IdorProfileEdit_Should_Return200_WhenAdminEditsOtherProfile()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _adminUserId;
        TestAuthHandler.CurrentRole = Auth.AdminRole;

        // Act
        var response = await _client.PutAsync($"/test/auth/profile/{_clientUserId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_IdorProfileEdit_Should_Return403_WhenClientEditsOtherProfile()
    {
        // Arrange
        var restrictedClientId = Guid.NewGuid();
        _factory.PermissionService.SetPermissions(restrictedClientId);
        TestAuthHandler.CurrentUserId = restrictedClientId;
        TestAuthHandler.CurrentRole = Auth.ClientRole;

        // Act
        var response = await _client.PutAsync($"/test/auth/profile/{_otherUserId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_AdminCreate_Should_Return200_WhenAdminCreates()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _adminUserId;
        TestAuthHandler.CurrentRole = Auth.AdminRole;

        // Act
        var response = await _client.PostAsync("/test/auth/admin-create", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoint_AdminCreate_Should_Return403_WhenClientTries()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _clientUserId;
        TestAuthHandler.CurrentRole = Auth.ClientRole;

        // Act
        var response = await _client.PostAsync("/test/auth/admin-create", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Endpoint_WhoAmI_Should_ReturnAllAdminPermissions()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _adminUserId;
        TestAuthHandler.CurrentRole = Auth.AdminRole;

        // Act
        var response = await _client.GetAsync("/test/auth/whoami");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<WhoAmIResponse>();
        content!.UserId.Should().Be(_adminUserId.ToString());
        content.Permissions.Should().Contain(Permission.AdminView.ToString());
        content.Permissions.Should().Contain(Permission.ClientEdit.ToString());
        content.Permissions.Should().HaveCount(8);
    }

    [Fact]
    public async Task Endpoint_WhoAmI_Should_ReturnOnlyClientPermissions()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _clientUserId;
        TestAuthHandler.CurrentRole = Auth.ClientRole;

        // Act
        var response = await _client.GetAsync("/test/auth/whoami");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<WhoAmIResponse>();
        content!.UserId.Should().Be(_clientUserId.ToString());
        content.Permissions.Should().Contain(Permission.ClientView.ToString());
        content.Permissions.Should().Contain(Permission.ClientEdit.ToString());
        content.Permissions.Should().NotContain(Permission.AdminView.ToString());
        content.Permissions.Should().HaveCount(4);
    }

    [Fact]
    public async Task Endpoint_PublicAuthenticated_Should_Return200_ForAnyAuthenticatedUser()
    {
        // Arrange
        TestAuthHandler.CurrentUserId = _otherUserId;
        TestAuthHandler.CurrentRole = Auth.GuestRole;

        // Act
        var response = await _client.GetAsync("/test/auth/public-authenticated");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private record TestResponse(
        string Message, 
        string UserId, 
        string Endpoint, 
        string? RequiredPermission);
    
    private record IdorTestResponse(
        string Message,
        string CurrentUserId,
        Guid RequestedUserId,
        bool IsOwner,
        string Endpoint,
        string IdorProtection);
    
    private record WhoAmIResponse(
        string UserId,
        string[] Roles,
        string[] Permissions,
        string Message);
}
