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
