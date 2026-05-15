namespace Venue.TimeCafe.Domain.Errors;

public sealed class TariffNotFoundError : Error
{
    public TariffNotFoundError() : base("Тариф не найден")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public sealed class TariffCreateFailedError : Error
{
    public TariffCreateFailedError() : base("Не удалось создать тариф")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class TariffUpdateFailedError : Error
{
    public TariffUpdateFailedError() : base("Не удалось обновить тариф")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class TariffDeleteFailedError : Error
{
    public TariffDeleteFailedError() : base("Не удалось удалить тариф")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class TariffActivateFailedError : Error
{
    public TariffActivateFailedError() : base("Не удалось активировать тариф")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class TariffDeactivateFailedError : Error
{
    public TariffDeactivateFailedError() : base("Не удалось деактивировать тариф")
    {
        Metadata.Add("ErrorCode", "500");
    }
}
