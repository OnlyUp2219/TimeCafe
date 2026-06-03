namespace Venue.TimeCafe.Domain.Errors;

public sealed class ThemeNotFoundError : Error
{
    public ThemeNotFoundError() : base("Тема не найдена")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public sealed class ThemeCreateFailedError : Error
{
    public ThemeCreateFailedError() : base("Не удалось создать тему")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class ThemeUpdateFailedError : Error
{
    public ThemeUpdateFailedError() : base("Не удалось обновить тему")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class ThemeDeleteFailedError : Error
{
    public ThemeDeleteFailedError() : base("Не удалось удалить тему")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class ThemeInUseError : Error
{
    public ThemeInUseError() : base("Нельзя удалить тему, так как она используется в тарифах")
    {
        Metadata.Add("ErrorCode", "409");
    }
}
