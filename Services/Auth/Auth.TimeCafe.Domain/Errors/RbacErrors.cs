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
