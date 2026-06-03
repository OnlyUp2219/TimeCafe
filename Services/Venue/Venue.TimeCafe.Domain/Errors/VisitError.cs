namespace Venue.TimeCafe.Domain.Errors;

public sealed class VisitNotFoundError : Error
{
    public VisitNotFoundError()
        : base("Посещение не найдено.")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public sealed class EndVisitFailedError : Error
{
    public EndVisitFailedError()
        : base("Не удалось завершить посещение.")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class CheckVisitFailedError : Error
{
    public CheckVisitFailedError()
        : base("Не удалось проверить активное посещение")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class CreateVisitFailedError : Error
{
    public CreateVisitFailedError()
        : base("Не удалось начать посещение.")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class VisitUpdateFailedError : Error
{
    public VisitUpdateFailedError()
        : base("Не удалось обновить посещение.")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class VisitDeleteFailedError : Error
{
    public VisitDeleteFailedError()
        : base("Не удалось удалить посещение.")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class VisitNotPendingError : Error
{
    public VisitNotPendingError()
        : base("Операция доступна только для визитов в статусе 'Ожидает подтверждения'.")
    {
        Metadata.Add("ErrorCode", "409");
    }
}

public sealed class VisitCannotBeCancelledError : Error
{
    public VisitCannotBeCancelledError()
        : base("Визит можно отменить только в статусе 'Ожидает подтверждения'.")
    {
        Metadata.Add("ErrorCode", "409");
    }
}

public sealed class VisitNotActiveError : Error
{
    public VisitNotActiveError()
        : base("Операция доступна только для активных визитов.")
    {
        Metadata.Add("ErrorCode", "409");
    }
}

public sealed class VisitAccessDeniedError : Error
{
    public VisitAccessDeniedError()
        : base("Доступ запрещен. Нельзя завершить чужой визит.")
    {
        Metadata.Add("ErrorCode", "403");
    }
}
