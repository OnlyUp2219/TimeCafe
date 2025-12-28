namespace UserProfile.TimeCafe.Test.Unit.Authorization;

public class PermissionRequirementTests
{
    [Fact]
    public void Constructor_SinglePermission_Should_SetPermissionAndDefaultMode()
    {
        // Arrange & Act
        var requirement = new PermissionRequirement(Permission.AdminView);

        // Assert
        requirement.Permissions.Should().ContainSingle()
            .Which.Should().Be(Permission.AdminView);
        requirement.Mode.Should().Be(PermissionCheckMode.Any);
    }

    [Fact]
    public void Constructor_MultiplePermissions_AnyMode_Should_SetCorrectly()
    {
        // Arrange & Act
        var requirement = new PermissionRequirement(
            PermissionCheckMode.Any, 
            Permission.AdminView, 
            Permission.ClientView);

        // Assert
        requirement.Permissions.Should().HaveCount(2);
        requirement.Permissions.Should().Contain(Permission.AdminView);
        requirement.Permissions.Should().Contain(Permission.ClientView);
        requirement.Mode.Should().Be(PermissionCheckMode.Any);
    }

    [Fact]
    public void Constructor_MultiplePermissions_AllMode_Should_SetCorrectly()
    {
        // Arrange & Act
        var requirement = new PermissionRequirement(
            PermissionCheckMode.All, 
            Permission.ClientView, 
            Permission.ClientEdit);

        // Assert
        requirement.Permissions.Should().HaveCount(2);
        requirement.Mode.Should().Be(PermissionCheckMode.All);
    }
}

public class PermissionEnumTests
{
    [Fact]
    public void Permission_Should_HaveClientPermissions()
    {
        // Assert
        Enum.IsDefined(typeof(Permission), Permission.ClientView).Should().BeTrue();
        Enum.IsDefined(typeof(Permission), Permission.ClientEdit).Should().BeTrue();
        Enum.IsDefined(typeof(Permission), Permission.ClientDelete).Should().BeTrue();
        Enum.IsDefined(typeof(Permission), Permission.ClientCreate).Should().BeTrue();
    }

    [Fact]
    public void Permission_Should_HaveAdminPermissions()
    {
        // Assert
        Enum.IsDefined(typeof(Permission), Permission.AdminView).Should().BeTrue();
        Enum.IsDefined(typeof(Permission), Permission.AdminEdit).Should().BeTrue();
        Enum.IsDefined(typeof(Permission), Permission.AdminDelete).Should().BeTrue();
        Enum.IsDefined(typeof(Permission), Permission.AdminCreate).Should().BeTrue();
    }

    [Fact]
    public void Permission_Should_HaveExactly8Permissions()
    {
        // Assert
        Enum.GetValues<Permission>().Should().HaveCount(Auth.AdminPermissionCount);
    }

    [Fact]
    public void Permission_ClientValues_Should_StartFrom100()
    {
        // Assert
        ((int)Permission.ClientView).Should().BeGreaterThanOrEqualTo(100);
        ((int)Permission.ClientView).Should().BeLessThan(200);
    }

    [Fact]
    public void Permission_AdminValues_Should_StartFrom200()
    {
        // Assert
        ((int)Permission.AdminView).Should().BeGreaterThanOrEqualTo(200);
        ((int)Permission.AdminView).Should().BeLessThan(300);
    }
}

public class PermissionCheckModeTests
{
    [Fact]
    public void PermissionCheckMode_Should_HaveAnyAndAllValues()
    {
        // Assert
        Enum.IsDefined(typeof(PermissionCheckMode), PermissionCheckMode.Any).Should().BeTrue();
        Enum.IsDefined(typeof(PermissionCheckMode), PermissionCheckMode.All).Should().BeTrue();
    }

    [Fact]
    public void PermissionCheckMode_Should_HaveExactlyTwoValues()
    {
        // Assert
        Enum.GetValues<PermissionCheckMode>().Should().HaveCount(2);
    }
}
