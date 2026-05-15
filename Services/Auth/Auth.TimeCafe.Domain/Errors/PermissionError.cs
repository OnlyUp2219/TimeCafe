namespace Auth.TimeCafe.Domain.Errors;

public sealed class PermissionsNotFoundError : Error
{
    public PermissionsNotFoundError()
        : base("Разрешения не найдены, повторите позже!")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public sealed class PermissionNotFoundError : Error
{
    public PermissionNotFoundError(string permission)
        : base($"Разрешение '{permission}' не найдено")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("Permission", permission);
    }
}
