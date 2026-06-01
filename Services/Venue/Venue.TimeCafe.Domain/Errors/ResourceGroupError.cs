namespace Venue.TimeCafe.Domain.Errors;

public sealed class ResourceGroupNotFoundError : Error
{
    public ResourceGroupNotFoundError() : base("Зона не найдена")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public sealed class ResourceGroupCreateFailedError : Error
{
    public ResourceGroupCreateFailedError() : base("Не удалось создать зону")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class ResourceGroupUpdateFailedError : Error
{
    public ResourceGroupUpdateFailedError() : base("Не удалось обновить зону")
    {
        Metadata.Add("ErrorCode", "500");
    }
}

public sealed class ResourceGroupDeleteFailedError : Error
{
    public ResourceGroupDeleteFailedError() : base("Не удалось удалить зону")
    {
        Metadata.Add("ErrorCode", "500");
    }
}
