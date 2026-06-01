namespace Venue.TimeCafe.Domain.Errors;

public sealed class ResourceNotFoundError : Error
{
    public ResourceNotFoundError() : base("Стол не найден")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public sealed class ResourceCreateFailedError : Error
{
    public ResourceCreateFailedError() : base("Не удалось создать стол")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class ResourceUpdateFailedError : Error
{
    public ResourceUpdateFailedError() : base("Не удалось обновить стол")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class ResourceDeleteFailedError : Error
{
    public ResourceDeleteFailedError() : base("Не удалось удалить стол")
    {
        Metadata.Add("ErrorCode", "500");
    }
}
