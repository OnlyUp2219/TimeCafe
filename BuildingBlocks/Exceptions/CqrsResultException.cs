namespace BuildingBlocks.Exceptions;

public class CqrsResultException : Exception
{
    public ICqrsResultV2? Result { get; }

    public CqrsResultException()
    {
    }

    public CqrsResultException(string? message) : base(message)
    {
    }

    public CqrsResultException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public CqrsResultException(ICqrsResultV2 result) : base(
        $"Name:{result.Code} " +
        $"Message:{result.Message} " +
        $"Errors:{string.Join("; ", (result.Errors ?? []).Select(e => $"{e.Code}: {e.Description}"))}")
    {
        Result = result;
    }

    public CqrsResultException(ICqrsResultV2 result, Exception exception) : base(
        $"Name:{result.Code} " +
        $"Message:{result.Message} " +
        $"Errors:{string.Join("; ", (result.Errors ?? []).Select(e => $"{e.Code}: {e.Description}"))}",
        exception)
    {
        Result = result;
    }
}
