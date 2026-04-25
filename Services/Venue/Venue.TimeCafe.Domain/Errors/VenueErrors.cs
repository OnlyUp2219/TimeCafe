namespace Venue.TimeCafe.Domain.Errors;

public class PromotionInvalidTypeError : Error
{
    public PromotionInvalidTypeError()
        : base("Для акции, привязанной к тарифу, необходимо указать TariffId, а для глобальной — TariffId должен быть пустым.")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public class VisitNotFoundError : Error
{
    public VisitNotFoundError()
        : base("Посещение не найдено.")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public class EndVisitFailedError : Error
{
    public EndVisitFailedError()
        : base("Не удалось завершить посещение.")
    {
        Metadata.Add("ErrorCode", "500");
    }
}


public class PromotionNotFoundError : Error
{
    public PromotionNotFoundError() : base("Акция не найдена")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public class ActivateFailedError : Error
{
    public ActivateFailedError() : base("Не удалось активировать акцию")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public class DeactivateFailedError : Error
{
    public DeactivateFailedError() : base("Не удалось деактивировать акцию")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public class DeleteFailedError : Error
{
    public DeleteFailedError() : base("Не удалось удалить акцию")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public class GetFailedError : Error
{
    public GetFailedError() : base("Не удалось получить активные акции на дату")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public class TariffNotFoundError : Error
{
    public TariffNotFoundError() : base("Тариф не найден")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public class ThemeNotFoundError : Error
{
    public ThemeNotFoundError() : base("Тема не найдена")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public class CreateFailedError : Error
{
    public CreateFailedError() : base("Не удалось создать тариф")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public class UpdateFailedError : Error
{
    public UpdateFailedError() : base("Не удалось обновить тариф")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public class InsufficientFundsError : Error
{
    public InsufficientFundsError() : base("Произошла ошибка")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public class BalanceCheckFailedError : Error
{
    public BalanceCheckFailedError() : base("Не удалось проверить баланс. Повторите попытку позже")
    {
        Metadata.Add("ErrorCode", "503");
    }
}

public class CheckFailedError : Error
{
    public CheckFailedError() : base("Не удалось проверить активное посещение")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public class ActiveGlobalPromotionAlreadyExistsError : Error
{
    public ActiveGlobalPromotionAlreadyExistsError()
        : base("Уже существует активная глобальная акция. Сначала деактивируйте её.")
    {
        Metadata.Add("ErrorCode", "409");
    }
}
