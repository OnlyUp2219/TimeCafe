namespace Venue.TimeCafe.Domain.Errors;

public sealed class PromotionNotFoundError : Error
{
    public PromotionNotFoundError() : base("Акция не найдена")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public sealed class PromotionInvalidTypeError : Error
{
    public PromotionInvalidTypeError()
        : base("Для акции, привязанной к тарифу, необходимо указать TariffId, а для глобальной — TariffId должен быть пустым.")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public sealed class PromotionActivateFailedError : Error
{
    public PromotionActivateFailedError() : base("Не удалось активировать акцию")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class PromotionDeactivateFailedError : Error
{
    public PromotionDeactivateFailedError() : base("Не удалось деактивировать акцию")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class PromotionDeleteFailedError : Error
{
    public PromotionDeleteFailedError() : base("Не удалось удалить акцию")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class PromotionGetFailedError : Error
{
    public PromotionGetFailedError() : base("Не удалось получить активные акции на дату")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class ActiveGlobalPromotionAlreadyExistsError : Error
{
    public ActiveGlobalPromotionAlreadyExistsError()
        : base("Уже существует активная глобальная акция. Сначала деактивируйте её.")
    {
        Metadata.Add("ErrorCode", "409");
    }
}
