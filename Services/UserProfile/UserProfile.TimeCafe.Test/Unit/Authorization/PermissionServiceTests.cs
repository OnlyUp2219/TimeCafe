namespace UserProfile.TimeCafe.Test.Unit.Authorization;

public class PermissionServiceTests
{
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly Guid _testUserId = Auth.DefaultUserId;

    public PermissionServiceTests()
    {
        _permissionServiceMock = new Mock<IPermissionService>();
    }

    [Fact]
    public async Task HasPermission_Should_ReturnTrue_WhenUserHasPermission()
    {
        // Arrange
        _permissionServiceMock
            .Setup(x => x.HasPermissionAsync(_testUserId, Permission.AdminView))
            .ReturnsAsync(true);

        // Act
        var result = await _permissionServiceMock.Object.HasPermissionAsync(_testUserId, Permission.AdminView);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermission_Should_ReturnFalse_WhenUserLacksPermission()
    {
        // Arrange
        _permissionServiceMock
            .Setup(x => x.HasPermissionAsync(_testUserId, Permission.AdminView))
            .ReturnsAsync(false);

        // Act
        var result = await _permissionServiceMock.Object.HasPermissionAsync(_testUserId, Permission.AdminView);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAnyPermission_Should_ReturnTrue_WhenUserHasAtLeastOnePermission()
    {
        // Arrange
        _permissionServiceMock
            .Setup(x => x.HasAnyPermissionAsync(_testUserId, Permission.AdminView, Permission.ClientView))
            .ReturnsAsync(true);

        // Act
        var result = await _permissionServiceMock.Object.HasAnyPermissionAsync(
            _testUserId, Permission.AdminView, Permission.ClientView);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnyPermission_Should_ReturnFalse_WhenUserHasNoPermissions()
    {
        // Arrange
        _permissionServiceMock
            .Setup(x => x.HasAnyPermissionAsync(_testUserId, Permission.AdminView, Permission.AdminEdit))
            .ReturnsAsync(false);

        // Act
        var result = await _permissionServiceMock.Object.HasAnyPermissionAsync(
            _testUserId, Permission.AdminView, Permission.AdminEdit);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAllPermissions_Should_ReturnTrue_WhenUserHasAllPermissions()
    {
        // Arrange
        _permissionServiceMock
            .Setup(x => x.HasAllPermissionsAsync(_testUserId, Permission.ClientView, Permission.ClientEdit))
            .ReturnsAsync(true);

        // Act
        var result = await _permissionServiceMock.Object.HasAllPermissionsAsync(
            _testUserId, Permission.ClientView, Permission.ClientEdit);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAllPermissions_Should_ReturnFalse_WhenUserMissingSomePermissions()
    {
        // Arrange
        _permissionServiceMock
            .Setup(x => x.HasAllPermissionsAsync(_testUserId, Permission.ClientView, Permission.AdminView))
            .ReturnsAsync(false);

        // Act
        var result = await _permissionServiceMock.Object.HasAllPermissionsAsync(
            _testUserId, Permission.ClientView, Permission.AdminView);

        // Assert
        result.Should().BeFalse();
    }
}

public class AlwaysAllowPermissionServiceTests
{
    private class TestAlwaysAllowService : IPermissionService
    {
        public Task<bool> HasPermissionAsync(Guid userId, Permission permission) => Task.FromResult(true);
        public Task<bool> HasAnyPermissionAsync(Guid userId, params Permission[] permissions) => Task.FromResult(true);
        public Task<bool> HasAllPermissionsAsync(Guid userId, params Permission[] permissions) => Task.FromResult(true);
    }

    private readonly IPermissionService _service = new TestAlwaysAllowService();
    private readonly Guid _testUserId = Auth.DefaultUserId;

    [Fact]
    public async Task HasPermission_Should_AlwaysReturnTrue()
    {
        // Act & Assert
        foreach (var permission in Enum.GetValues<Permission>())
        {
            var result = await _service.HasPermissionAsync(_testUserId, permission);
            result.Should().BeTrue();
        }
    }

    [Fact]
    public async Task HasAnyPermission_Should_AlwaysReturnTrue()
    {
        // Act
        var result = await _service.HasAnyPermissionAsync(
            _testUserId, Permission.AdminView, Permission.ClientView);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAllPermissions_Should_AlwaysReturnTrue()
    {
        // Act
        var result = await _service.HasAllPermissionsAsync(
            _testUserId, Permission.AdminView, Permission.ClientView, Permission.AdminCreate);

        // Assert
        result.Should().BeTrue();
    }
}

public class TestPermissionServiceTests
{
    [Fact]
    public async Task SetAsAdmin_Should_GrantAllPermissions()
    {
        // Arrange
        var service = new TestPermissionService();
        var userId = Auth.AdminUserId;

        // Act
        service.SetAsAdmin(userId);

        // Assert
        foreach (var permission in Enum.GetValues<Permission>())
        {
            (await service.HasPermissionAsync(userId, permission)).Should().BeTrue();
        }
    }

    [Fact]
    public async Task SetAsClient_Should_GrantOnlyClientPermissions()
    {
        // Arrange
        var service = new TestPermissionService();
        var userId = Auth.ClientUserId;

        // Act
        service.SetAsClient(userId);

        // Assert
        (await service.HasPermissionAsync(userId, Permission.ClientView)).Should().BeTrue();
        (await service.HasPermissionAsync(userId, Permission.ClientEdit)).Should().BeTrue();
        (await service.HasPermissionAsync(userId, Permission.ClientDelete)).Should().BeTrue();
        (await service.HasPermissionAsync(userId, Permission.ClientCreate)).Should().BeTrue();
        (await service.HasPermissionAsync(userId, Permission.AdminView)).Should().BeFalse();
        (await service.HasPermissionAsync(userId, Permission.AdminCreate)).Should().BeFalse();
    }

    [Fact]
    public async Task SetPermissions_Should_GrantSpecificPermissions()
    {
        // Arrange
        var service = new TestPermissionService();
        var userId = Auth.DefaultUserId;

        // Act
        service.SetPermissions(userId, Permission.ClientView, Permission.AdminCreate);

        // Assert
        (await service.HasPermissionAsync(userId, Permission.ClientView)).Should().BeTrue();
        (await service.HasPermissionAsync(userId, Permission.AdminCreate)).Should().BeTrue();
        (await service.HasPermissionAsync(userId, Permission.ClientEdit)).Should().BeFalse();
    }

    [Fact]
    public async Task HasAnyPermission_Should_ReturnTrue_WhenUserHasOneOfRequested()
    {
        // Arrange
        var service = new TestPermissionService();
        var userId = Auth.DefaultUserId;
        service.SetPermissions(userId, Permission.ClientView);

        // Act
        var result = await service.HasAnyPermissionAsync(userId, Permission.ClientView, Permission.AdminView);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAllPermissions_Should_ReturnFalse_WhenUserMissingSome()
    {
        // Arrange
        var service = new TestPermissionService();
        var userId = Auth.DefaultUserId;
        service.SetPermissions(userId, Permission.ClientView);

        // Act
        var result = await service.HasAllPermissionsAsync(userId, Permission.ClientView, Permission.ClientEdit);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Clear_Should_RemoveAllPermissions()
    {
        // Arrange
        var service = new TestPermissionService();
        var userId = Auth.AdminUserId;
        service.SetAsAdmin(userId);

        // Act
        service.Clear();

        // Assert
        (await service.HasPermissionAsync(userId, Permission.AdminView)).Should().BeFalse();
    }
}
