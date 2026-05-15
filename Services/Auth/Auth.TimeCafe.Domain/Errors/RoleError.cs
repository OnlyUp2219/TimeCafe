namespace Auth.TimeCafe.Domain.Errors;

public sealed class RolesNotFoundError : Error
{
    public RolesNotFoundError()
        : base("Роли не найдены, повторите позже!")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public sealed class RoleNotFoundError : Error
{
    public RoleNotFoundError(string roleName)
        : base($"Роль '{roleName}' не найдена")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("RoleName", roleName);
    }
}

public sealed class RoleClaimsNotFoundError : Error
{
    public RoleClaimsNotFoundError()
        : base("Связки ролей и разрешений не найдены")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public sealed class RoleExistError : Error
{
    public RoleExistError(string roleName)
        : base($"Роль '{roleName}' уже существует")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("RoleName", roleName);
    }
}

public sealed class SystemRoleModificationError : Error
{
    public SystemRoleModificationError(string roleName)
        : base($"Роль '{roleName}' является системной и не может быть изменена или удалена.")
    {
        Metadata.Add("ErrorCode", "403");
        Metadata.Add("Type", "SystemResource");
        Metadata.Add("RoleName", roleName);
    }
}

public sealed class SuperAdminModificationError : Error
{
    public SuperAdminModificationError()
        : base("Работать с ролью SuperAdmin через интерфейс нельзя. SuperAdmin может быть только один, он создается при инициализации системы.")
    {
        Metadata.Add("ErrorCode", "403");
    }
}

public sealed class LastRoleRemovalError : Error
{
    public LastRoleRemovalError()
        : base("Нельзя удалить последнюю роль пользователя. Сначала назначьте новую роль, а затем удалите старую.")
    {
        Metadata.Add("ErrorCode", "400");
    }
}
