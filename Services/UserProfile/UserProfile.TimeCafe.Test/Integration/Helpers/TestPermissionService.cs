using BuildingBlocks.Authorization;

namespace UserProfile.TimeCafe.Test.Integration.Helpers;

/// <summary>
/// Mock реализация IPermissionService для интеграционных тестов.
/// Позволяет настраивать разрешения для тестовых пользователей.
/// </summary>
public class TestPermissionService : IPermissionService
{
    private readonly Dictionary<Guid, HashSet<Permission>> _userPermissions = new();

    /// <summary>
    /// Настраивает разрешения для пользователя.
    /// </summary>
    public TestPermissionService SetPermissions(Guid userId, params Permission[] permissions)
    {
        _userPermissions[userId] = new HashSet<Permission>(permissions);
        return this;
    }

    /// <summary>
    /// Добавляет админские разрешения пользователю.
    /// </summary>
    public TestPermissionService SetAsAdmin(Guid userId)
    {
        _userPermissions[userId] = new HashSet<Permission>(Enum.GetValues<Permission>());
        return this;
    }

    /// <summary>
    /// Добавляет клиентские разрешения пользователю.
    /// </summary>
    public TestPermissionService SetAsClient(Guid userId)
    {
        _userPermissions[userId] =
        [
            Permission.ClientView,
            Permission.ClientEdit,
            Permission.ClientDelete,
            Permission.ClientCreate
        ];
        return this;
    }

    /// <summary>
    /// Очищает все разрешения.
    /// </summary>
    public TestPermissionService Clear()
    {
        _userPermissions.Clear();
        return this;
    }

    public Task<bool> HasPermissionAsync(Guid userId, Permission permission)
    {
        return Task.FromResult(
            _userPermissions.TryGetValue(userId, out var perms) && perms.Contains(permission));
    }

    public Task<bool> HasAnyPermissionAsync(Guid userId, params Permission[] permissions)
    {
        if (!_userPermissions.TryGetValue(userId, out var perms))
            return Task.FromResult(false);

        return Task.FromResult(permissions.Any(p => perms.Contains(p)));
    }

    public Task<bool> HasAllPermissionsAsync(Guid userId, params Permission[] permissions)
    {
        if (!_userPermissions.TryGetValue(userId, out var perms))
            return Task.FromResult(false);

        return Task.FromResult(permissions.All(p => perms.Contains(p)));
    }
}
