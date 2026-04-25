using FluentResults;

namespace UserProfile.TimeCafe.Domain.Errors;

public class ProfileNotFoundError : Error
{
    public ProfileNotFoundError() : base("Пользователь не найден")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public class InfoNotFoundError : Error
{
    public InfoNotFoundError() : base("Информация не найдена")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public class ProfileAlreadyExistsError : Error
{
    public ProfileAlreadyExistsError() : base("Пользователь уже существует")
    {
        Metadata.Add("ErrorCode", "409");
    }
}

public class UpdateFailedError : Error
{
    public UpdateFailedError() : base("Ошибка обновления")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public class CreateFailedError : Error
{
    public CreateFailedError() : base("Ошибка создания")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public class PhotoNotFoundError : Error
{
    public PhotoNotFoundError() : base("Фотография не найдена")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public class FailedError : Error
{
    public FailedError() : base("Ошибка операции")
    {
        Metadata.Add("ErrorCode", "500");
    }
}
