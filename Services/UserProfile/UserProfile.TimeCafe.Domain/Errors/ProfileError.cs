namespace UserProfile.TimeCafe.Domain.Errors;

public sealed class ProfileNotFoundError : Error
{
    public ProfileNotFoundError() : base("Пользователь не найден")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public sealed class ProfileAlreadyExistsError : Error
{
    public ProfileAlreadyExistsError() : base("Пользователь уже существует")
    {
        Metadata.Add("ErrorCode", "409");
    }
}

public sealed class UpdateFailedError : Error
{
    public UpdateFailedError() : base("Ошибка обновления")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class CreateFailedError : Error
{
    public CreateFailedError() : base("Ошибка создания")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class FailedError : Error
{
    public FailedError() : base("Ошибка операции")
    {
        Metadata.Add("ErrorCode", "500");
    }
}
