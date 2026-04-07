namespace Auth.TimeCafe.Domain.Errors;

public class PermissionsNotFoundError : Error
{
    public PermissionsNotFoundError()
        : base($"Разрешения не найдены, повторите позже!")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public class PermissionNotFoundError : Error
{
    public PermissionNotFoundError(string permission)
        : base($"Разрешение '{permission}' не найдено")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("Permission", permission);
    }
}

public class RolesNotFoundError : Error
{
    public RolesNotFoundError()
        : base($"Роли не найдены, повторите позже!")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public class RoleNotFoundError : Error
{
    public RoleNotFoundError(string roleName)
        : base($"Роль '{roleName}' не найдена")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("RoleName", roleName);
    }
}

public class RoleClaimsNotFoundError : Error
{
    public RoleClaimsNotFoundError()
        : base("Связки ролей и разрешений не найдены")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public class RoleExistError : Error
{
    public RoleExistError(string roleName)
        : base($"Роль '{roleName}' уже существует")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("RoleName", roleName); 
    }
}

public class SystemRoleModificationError : Error
{
    public SystemRoleModificationError(string roleName)
        : base($"Роль '{roleName}' является системной и не может быть изменена или удалена.")
    {
        Metadata.Add("ErrorCode", "403"); 
        Metadata.Add("Type", "SystemResource");
        Metadata.Add("RoleName", roleName);
    }
}

public class UserNotFoundError : Error
{
    public UserNotFoundError(Guid userId)
        : base($"Пользователь '{userId}' не найден")
    {
        Metadata.Add("ErrorCode", "404");
        Metadata.Add("UserId", userId);
    }
}

public class UserAlreadyInRoleError : Error
{
    public UserAlreadyInRoleError(Guid userId, string roleName)
        : base($"Пользователь '{userId}' уже имеет роль '{roleName}'")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("UserId", userId);
        Metadata.Add("RoleName", roleName);
    }
}

public class UserRoleNotAssignedError : Error
{
    public UserRoleNotAssignedError(Guid userId, string roleName)
        : base($"У пользователя '{userId}' отсутствует роль '{roleName}'")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("UserId", userId);
        Metadata.Add("RoleName", roleName);
    }
}
