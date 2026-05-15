namespace Auth.TimeCafe.Domain.Errors;

public sealed class UserNotFoundError : Error
{
    public UserNotFoundError(Guid userId)
        : base($"Пользователь '{userId}' не найден")
    {
        Metadata.Add("ErrorCode", "404");
        Metadata.Add("UserId", userId);
    }
}

public sealed class UserAlreadyInRoleError : Error
{
    public UserAlreadyInRoleError(Guid userId, string roleName)
        : base($"Пользователь '{userId}' уже имеет роль '{roleName}'")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("UserId", userId);
        Metadata.Add("RoleName", roleName);
    }
}

public sealed class UserRoleNotAssignedError : Error
{
    public UserRoleNotAssignedError(Guid userId, string roleName)
        : base($"У пользователя '{userId}' отсутствует роль '{roleName}'")
    {
        Metadata.Add("ErrorCode", "400");
        Metadata.Add("UserId", userId);
        Metadata.Add("RoleName", roleName);
    }
}
